import bpy
import bmesh
import math
import numpy

STREET_HALF_WIDTH = .7
NODE_FROM_CURB = .2
STREET_CURB_HEIGHT = .02
#STREET_LIGHT_DISTANCE = 6
#STREET_LIGHT_BUCKET_SIZE = 0.919 # To eliminate street lights that are too close together. This is the size for each dimension of the bucket.
#STREET_LIGHT_MIN_DISTANCE = 1.3
#streetLightBucket = {}

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
'''
def checkStreetLightBucket(bucket, location):
	global streetLightBucket
	if bucket in streetLightBucket:
		for lightLocation in streetLightBucket[bucket]:
			if distance(location, lightLocation) < STREET_LIGHT_MIN_DISTANCE:
				return False
		
		return True
	else:
		return True #There's nothing within the min distance because there's nothing in the bucket.
'''
#Create empty object where the street light is supposed to be.
def createObject(name, location):
    '''
	global streetLightBucket
	bucket = (int(location[0] / STREET_LIGHT_BUCKET_SIZE), int(location[1] / STREET_LIGHT_BUCKET_SIZE))
	if bucket not in streetLightBucket:
		if checkStreetLightBucket((bucket[0] - 1, bucket[1] - 1), location) and \
				checkStreetLightBucket((bucket[0], bucket[1] - 1), location) and \
				checkStreetLightBucket((bucket[0] + 1, bucket[1] - 1), location) and \
				checkStreetLightBucket((bucket[0] - 1, bucket[1]), location) and \
				checkStreetLightBucket((bucket[0] + 1, bucket[1]), location) and \
				checkStreetLightBucket((bucket[0] - 1, bucket[1] + 1), location) and \
				checkStreetLightBucket((bucket[0], bucket[1] + 1), location) and \
				checkStreetLightBucket((bucket[0] + 1, bucket[1] + 1), location) and \
				checkStreetLightBucket((bucket[0] - 2, bucket[1]), location) and \
				checkStreetLightBucket((bucket[0] + 2, bucket[1]), location) and \
				checkStreetLightBucket((bucket[0], bucket[1] - 2), location) and \
				checkStreetLightBucket((bucket[0], bucket[1] + 2), location):
			newObject = bpy.data.objects.new(name, None)
			newObject.location = location
			bpy.context.scene.objects.link(newObject);
			appendDict(streetLightBucket, bucket, location)
			return newObject
		else:
			return None
	else:
		return None
    '''
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
    appendDict(connectionDict, edge.vertices[0], edge.vertices[1])
    appendDict(connectionDict, edge.vertices[1], edge.vertices[0])

print("Finished adding edge dictionary. Creating vertex-to-vertex map.")

edgeNodeDict = {}
nodeCornerList = []
nodeConnectionDict = {}
for vertex in list(connectionDict.keys()):
    thisLocation = streetSkeletonVertices[vertex].co
    listOfConnections = connectionDict[vertex]
    listOfConnections.sort(key = lambda x: math.atan2(streetSkeletonVertices[x].co[1] - thisLocation[1],
        streetSkeletonVertices[x].co[0] - thisLocation[0]))
    
    if len(listOfConnections) == 1:
        otherLocation = streetSkeletonVertices[listOfConnections[0]].co
        vector = normalize((otherLocation[1] - thisLocation[1], thisLocation[0] - otherLocation[0]))
        vectorStreetLight = (vector[0] * (STREET_HALF_WIDTH + NODE_FROM_CURB), vector[1] * (STREET_HALF_WIDTH + NODE_FROM_CURB))
        rightCorner = (thisLocation[0] + vectorStreetLight[0], thisLocation[1] + vectorStreetLight[1], thisLocation[2] + STREET_CURB_HEIGHT)
        leftCorner = (thisLocation[0] - vectorStreetLight[0], thisLocation[1] - vectorStreetLight[1], thisLocation[2] + STREET_CURB_HEIGHT)
        nodeCornerList.append(rightCorner)
        nodeCornerList.append(leftCorner)
        corners = (len(nodeCornerList) - 2, len(nodeCornerList) - 1)
        edgeNodeDict.update({(vertex, listOfConnections[0]):corners})
    else:
        cornersNode = []
        for i in range(len(listOfConnections)):
            otherLocation = streetSkeletonVertices[listOfConnections[i]].co
            lastLocation = streetSkeletonVertices[listOfConnections[i - 1]].co
            
            currentRight = Line.lineFromPoints(thisLocation[0:2], otherLocation[0:2])
            lastLeft = Line.lineFromPoints(thisLocation[0:2], lastLocation[0:2])

            shiftRight(currentRight, STREET_HALF_WIDTH + NODE_FROM_CURB)
            shiftLeft(lastLeft, STREET_HALF_WIDTH + NODE_FROM_CURB)
            intersection = currentRight.intersect(lastLeft)
            if intersection == None:
                intersection = currentRight.point
            currentCorner = (intersection[0], intersection[1], thisLocation[2] + STREET_CURB_HEIGHT)
            nodeCornerList.append(currentCorner)
            cornersNode.append(len(nodeCornerList) - 1)
            
        for i in range(len(listOfConnections) - 1):
            edgeNodeDict.update({(vertex, listOfConnections[i]):(cornersNode[i], cornersNode[i + 1])})
        edgeNodeDict.update({(vertex, listOfConnections[-1]):(cornersNode[-1], cornersNode[0])})

print("Finished creating vertex-to-vertex map. Bridging streets.")


edgeDone = set()
nodeDone = set()
nodeConnectionDict = {}
for edgeKey in list(edgeNodeDict.keys()):
    if edgeKey not in edgeDone:
        otherKey = (edgeKey[1], edgeKey[0])
        
        currentRightNodeLocation = nodeCornerList[edgeNodeDict[edgeKey][0]]
        if edgeNodeDict[edgeKey][0] not in nodeDone:
            createObject("Pedestrian_Node" + str(edgeNodeDict[edgeKey][0]), currentRightNodeLocation)
            nodeDone.add(edgeNodeDict[edgeKey][0])

        currentLeftNodeLocation = nodeCornerList[edgeNodeDict[edgeKey][1]]
        if edgeNodeDict[edgeKey][1] not in nodeDone:
            createObject("Pedestrian_Node" + str(edgeNodeDict[edgeKey][1]), currentLeftNodeLocation)
            nodeDone.add(edgeNodeDict[edgeKey][1])

        otherRightNodeLocation = nodeCornerList[edgeNodeDict[otherKey][0]]
        if edgeNodeDict[otherKey][0] not in nodeDone:
            createObject("Pedestrian_Node" + str(edgeNodeDict[otherKey][0]), otherRightNodeLocation)
            nodeDone.add(edgeNodeDict[otherKey][0])

        otherLeftNodeLocation = nodeCornerList[edgeNodeDict[otherKey][1]]
        if edgeNodeDict[otherKey][1] not in nodeDone:
            createObject("Pedestrian_Node" + str(edgeNodeDict[otherKey][1]), otherLeftNodeLocation)
            nodeDone.add(edgeNodeDict[otherKey][1])
        
        appendDict(nodeConnectionDict, edgeNodeDict[edgeKey][0], edgeNodeDict[otherKey][1])
        appendDict(nodeConnectionDict, edgeNodeDict[otherKey][1], edgeNodeDict[edgeKey][0])
        appendDict(nodeConnectionDict, edgeNodeDict[edgeKey][1], edgeNodeDict[otherKey][0])
        appendDict(nodeConnectionDict, edgeNodeDict[otherKey][0], edgeNodeDict[edgeKey][1])
        
        edgeDone.add(edgeKey)
        edgeDone.add(otherKey)

#print(nodeConnectionDict)
print("Done!!!")