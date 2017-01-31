import bpy
import bmesh
import math
import numpy

STREET_HALF_WIDTH = .7
STREET_LIGHT_FROM_CURB = .2
STREET_CURB_HEIGHT = .02
STREET_LIGHT_DISTANCE = 6
FILL_LOOP = True

#Represented in form: point_P + t * vector_V
#Stores coordinate of point P and vector V in form of tuples
class Line:
    def __init__(self, point, vector):
        self.point = point
        self.vector = vector

    #Method to create a line from 2 points
    @staticmethod
    def lineFromPoints(point1, point2):
        deltaX = point2[0] - point1[0]
        deltaY = point2[1] - point1[1]
        return Line(point1, (deltaX, deltaY))
        
    #Method to find intersection between 2 lines
    def intersect(self, line):
        if self.vector == line.vector or self.vector == (-line.vector[0], -line.vector[1]): return None
        if self.point == line.point: return self.point
        vectorBA = (self.point[0] - line.point[0], self.point[1] - line.point[1])
        vectorBC = (-self.vector[1], self.vector[0])
        crossSelfBA = self.vector[0] * vectorBA[1] - self.vector[1] * vectorBA[0]
        if crossSelfBA < 0: vectorBC = (-vectorBC[0], -vectorBC[1])
        vectorBD = line.vector
        crossSelfBD = self.vector[0] * vectorBD[1] - self.vector[1] * vectorBD[0]
        if crossSelfBA * crossSelfBD < 0: vectorBD = (-vectorBD[0], -vectorBD[1])
        lengthBA = math.sqrt(vectorBA[0] ** 2 + vectorBA[1] ** 2)
        lengthVectorBC = math.sqrt(vectorBC[0] ** 2 + vectorBC[1] ** 2)
        cosTheta1 = (vectorBA[0] * vectorBC[0] + vectorBA[1] * vectorBC[1]) / (lengthBA * lengthVectorBC)
        lengthBC = cosTheta1 * lengthBA
        lengthVectorBD = math.sqrt(vectorBD[0] ** 2 + vectorBD[1] ** 2)
        cosTheta2 = (vectorBC[0] * vectorBD[0] + vectorBC[1] * vectorBD[1]) / (lengthVectorBC * lengthVectorBD)
        lengthBD = lengthBC / cosTheta2
        vectorBD = (lengthBD * vectorBD[0] / lengthVectorBD, lengthBD * vectorBD[1] / lengthVectorBD)
        return (line.point[0] + vectorBD[0], line.point[1] + vectorBD[1])

    def shift(self, vector):
        self.point = (self.point[0] + vector[0], self.point[1] + vector[1])

def normalize(vector):
    length = math.sqrt(vector[0] ** 2 + vector[1] ** 2)
    return (vector[0] / length, vector[1] / length)

def shiftRight(line, distance):
    vector = (line.vector[0], line.vector[1])
    vector = normalize(vector)
    vector = (distance * vector[1], distance * -vector[0])
    line.shift(vector)

def shiftLeft(line, distance):
    shiftRight(line, -distance)

def appendDict(dict, key, value):
    if key not in dict:
        dict.update({key:[value]})
    else:
        dict[key].append(value)

def distance(point1, point2):
    return math.sqrt((point1[0] - point2[0]) ** 2 + (point1[1] - point2[1]) ** 2 + (point1[2] - point2[2]) ** 2)

def createObject(name, location):
    newObject = bpy.data.objects.new(name, None)
    newObject.location = location
    bpy.context.scene.objects.link(newObject);
    return newObject

def lerp(num1, num2, alpha):
    return (1 - alpha) * num1 + alpha * num2
    
def lerpLocation(location1, location2, alpha):
    return (lerp(location1[0], location2[0], alpha), lerp(location1[1], location2[1], alpha),\
            lerp(location1[2], location2[2], alpha))

def fillInObjects(name, location1, location2, alphaInterval):
    for i in numpy.arange(alphaInterval, 1, alphaInterval):
        createObject(name, lerpLocation(location1, location2, i))

streetSkeleton = bpy.context.selected_objects[0]
streetSkeletonVertices = streetSkeleton.data.vertices

connectionDict = {}
for edge in streetSkeleton.data.edges:
    if edge.vertices[0] not in connectionDict:
        connectionDict.update({edge.vertices[0]:[edge.vertices[1]]})
    else:
        connectionDict[edge.vertices[0]].append(edge.vertices[1])
    
    if edge.vertices[1] not in connectionDict:
        connectionDict.update({edge.vertices[1]:[edge.vertices[0]]})
    else:
        connectionDict[edge.vertices[1]].append(edge.vertices[0])

print("Finished adding edge dictionary. Creating vertex-to-vertex map.")

singleCorners = set()

edgeStreetLightDict = {}
lightCornerList = []
lightConnectionDict = {}
for vertex in list(connectionDict.keys()):
    thisLocation = streetSkeletonVertices[vertex].co
    listOfConnections = connectionDict[vertex]
    listOfConnections.sort(key = lambda x: math.atan2(streetSkeletonVertices[x].co[1] - thisLocation[1],
        streetSkeletonVertices[x].co[0] - thisLocation[0]))
    
    if len(listOfConnections) == 1:
        otherLocation = streetSkeletonVertices[listOfConnections[0]].co
        vector = normalize((otherLocation[1] - thisLocation[1], thisLocation[0] - otherLocation[0]))
        vectorStreetLight = (vector[0] * (STREET_HALF_WIDTH + STREET_LIGHT_FROM_CURB), vector[1] * (STREET_HALF_WIDTH + STREET_LIGHT_FROM_CURB))
        rightCorner = (thisLocation[0] + vectorStreetLight[0], thisLocation[1] + vectorStreetLight[1], thisLocation[2] + STREET_CURB_HEIGHT)
        leftCorner = (thisLocation[0] - vectorStreetLight[0], thisLocation[1] - vectorStreetLight[1], thisLocation[2] + STREET_CURB_HEIGHT)
        lightCornerList.append(rightCorner)
        lightCornerList.append(leftCorner)
        corners = (len(lightCornerList) - 2, len(lightCornerList) - 1)
        edgeStreetLightDict.update({(vertex, listOfConnections[0]):corners})
    else:
        cornersStreetLight = []
        for i in range(len(listOfConnections)):
            otherLocation = streetSkeletonVertices[listOfConnections[i]].co
            lastLocation = streetSkeletonVertices[listOfConnections[i - 1]].co
            
            currentRight = Line.lineFromPoints(thisLocation[0:2], otherLocation[0:2])
            lastLeft = Line.lineFromPoints(thisLocation[0:2], lastLocation[0:2])

            shiftRight(currentRight, STREET_HALF_WIDTH + STREET_LIGHT_FROM_CURB)
            shiftLeft(lastLeft, STREET_HALF_WIDTH + STREET_LIGHT_FROM_CURB)
            intersection = currentRight.intersect(lastLeft)
            if intersection == None:
                intersection = currentRight.point
            currentCorner = (intersection[0], intersection[1], thisLocation[2] + STREET_CURB_HEIGHT)
            lightCornerList.append(currentCorner)
            cornersStreetLight.append(len(lightCornerList) - 1)
            
        for i in range(len(listOfConnections) - 1):
            edgeStreetLightDict.update({(vertex, listOfConnections[i]):(cornersStreetLight[i], cornersStreetLight[i + 1])})
        edgeStreetLightDict.update({(vertex, listOfConnections[-1]):(cornersStreetLight[-1], cornersStreetLight[0])})

print("Finished creating vertex-to-vertex map. Bridging streets.")

edgeDone = set()
lightCornerDone = set()
for edgeKey in list(edgeStreetLightDict.keys()):
    if edgeKey not in edgeDone:
        otherKey = (edgeKey[1], edgeKey[0])
        
        currentRightLightLocation = lightCornerList[edgeStreetLightDict[edgeKey][0]]
        if edgeStreetLightDict[edgeKey][0] not in lightCornerDone:
            createObject("Street_Light", currentRightLightLocation)
            lightCornerDone.add(edgeStreetLightDict[edgeKey][0])

        currentLeftLightLocation = lightCornerList[edgeStreetLightDict[edgeKey][1]]
        if edgeStreetLightDict[edgeKey][1] not in lightCornerDone:
            createObject("Street_Light", currentLeftLightLocation)
            lightCornerDone.add(edgeStreetLightDict[edgeKey][1])

        otherRightLightLocation = lightCornerList[edgeStreetLightDict[otherKey][0]]
        if edgeStreetLightDict[otherKey][0] not in lightCornerDone:
            createObject("Street_Light", otherRightLightLocation)
            lightCornerDone.add(edgeStreetLightDict[otherKey][0])

        otherLeftLightLocation = lightCornerList[edgeStreetLightDict[otherKey][1]]
        if edgeStreetLightDict[otherKey][1] not in lightCornerDone:
            createObject("Street_Light", otherLeftLightLocation)
            lightCornerDone.add(edgeStreetLightDict[otherKey][1])
                
        rightDistance = distance(currentRightLightLocation, otherLeftLightLocation)
        leftDistance = distance(currentLeftLightLocation, otherRightLightLocation)
        
        numberOfRightLights = round(rightDistance / STREET_LIGHT_DISTANCE)
        if numberOfRightLights > 0:
            rightAlphaInterval = 1 / numberOfRightLights
            fillInObjects("Street_Light", currentRightLightLocation, otherLeftLightLocation, rightAlphaInterval)
        
        numberOfLeftLights = round(leftDistance / STREET_LIGHT_DISTANCE)
        if numberOfLeftLights > 0:
            leftAlphaInterval = 1 / numberOfLeftLights
            fillInObjects("Street_Light", currentLeftLightLocation, otherRightLightLocation, leftAlphaInterval)
        
        edgeDone.add(edgeKey)
        edgeDone.add(otherKey)

print("Done!!!")