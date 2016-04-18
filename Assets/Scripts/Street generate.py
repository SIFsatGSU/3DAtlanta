import bpy
import math
import bmesh
STREET_HALF_WIDTH = .7
STREET_SIDE_WALK_WIDTH = .2
STREET_CURB_HEIGHT = .02
CORNER_BEVEL_OFFSET = 1
CORNER_BEVEL_SEGMENTS = 24
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

def createPolygon(mesh, vertexList):
    mesh.polygons.add(1)
    for vertex in vertexList:
        mesh.loops.add(1)
        mesh.loops[-1].vertex_index = vertex
    mesh.polygons[-1].loop_start = mesh.loops[-len(vertexList)].index
    mesh.polygons[-1].loop_total = len(vertexList)

def appendDict(dict, key, value):
    if key not in dict:
        dict.update({key:[value]})
    else:
        dict[key].append(value)

streetSkeleton = bpy.context.selected_objects[0]
streetSkeletonVertices = streetSkeleton.data.vertices
streetObject = None
if bpy.data.objects.get("Street") is None:
    streetObject = bpy.data.objects.new("Street", bpy.data.meshes.new("Street"))
    bpy.context.scene.objects.link(streetObject)
else:
    streetObject = bpy.data.objects["Street"]
    streetObject.data = bpy.data.meshes.new("Street")
    bpy.data.meshes.remove(bpy.data.meshes["Street"])
    streetObject.data.name = "Street"

streetObject.location = streetSkeleton.location
streetObject.scale = streetSkeleton.scale
streetObject.rotation_euler = streetSkeleton.rotation_euler
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

verticleEdges = {}
edgePlaneDict = {}

for vertex in list(connectionDict.keys()):
    thisLocation = streetSkeletonVertices[vertex].co
    listOfConnections = connectionDict[vertex]
    listOfConnections.sort(key = lambda x: math.atan2(streetSkeletonVertices[x].co[1] - thisLocation[1],
        streetSkeletonVertices[x].co[0] - thisLocation[0]))
    
    if len(listOfConnections) == 1:
        otherLocation = streetSkeletonVertices[listOfConnections[0]].co
        vector = normalize((otherLocation[1] - thisLocation[1], thisLocation[0] - otherLocation[0]))
        vectorSideWalk = (vector[0] * (STREET_HALF_WIDTH + STREET_SIDE_WALK_WIDTH), vector[1] * (STREET_HALF_WIDTH + STREET_SIDE_WALK_WIDTH))
        vector = (vector[0] * STREET_HALF_WIDTH, vector[1] * STREET_HALF_WIDTH)
        streetObject.data.vertices.add(6)
        streetObject.data.vertices[-6].co = (thisLocation[0] + vector[0], thisLocation[1] + vector[1], thisLocation[2])
        streetObject.data.vertices[-5].co = (thisLocation[0] + vector[0], thisLocation[1] + vector[1], thisLocation[2] + STREET_CURB_HEIGHT)
        streetObject.data.vertices[-4].co = (thisLocation[0] + vectorSideWalk[0], thisLocation[1] + vectorSideWalk[1], thisLocation[2] + STREET_CURB_HEIGHT)
        streetObject.data.vertices[-3].co = (thisLocation[0] - vector[0], thisLocation[1] - vector[1], thisLocation[2])
        streetObject.data.vertices[-2].co = (thisLocation[0] - vector[0], thisLocation[1] - vector[1], thisLocation[2] + STREET_CURB_HEIGHT)        
        streetObject.data.vertices[-1].co = (thisLocation[0] - vectorSideWalk[0], thisLocation[1] - vectorSideWalk[1], thisLocation[2] + STREET_CURB_HEIGHT)        
        corners = tuple([vertex.index for vertex in streetObject.data.vertices[-6:]])
        edgePlaneDict.update({(vertex, listOfConnections[0]):corners})
        singleCorners.add((vertex, listOfConnections[0]))
    else:
        corners = []
        cornersCurb = []
        cornersSideWalk = []
        for i in range(len(listOfConnections)):
            otherLocation = streetSkeletonVertices[listOfConnections[i]].co
            lastLocation = streetSkeletonVertices[listOfConnections[i - 1]].co
            streetObject.data.vertices.add(3)
            corners.append(streetObject.data.vertices[-3].index)
            cornersCurb.append(streetObject.data.vertices[-2].index)
            cornersSideWalk.append(streetObject.data.vertices[-1].index)
            currentCorner = streetObject.data.vertices[-3]
            currentCornerCurb = streetObject.data.vertices[-2]
            currentCornerSideWalk = streetObject.data.vertices[-1]
            currentRight = Line.lineFromPoints(thisLocation[0:2], otherLocation[0:2])
            lastLeft = Line.lineFromPoints(thisLocation[0:2], lastLocation[0:2])

            shiftRight(currentRight, STREET_HALF_WIDTH)
            shiftLeft(lastLeft, STREET_HALF_WIDTH)
            intersection = currentRight.intersect(lastLeft)
            if intersection == None:
                intersection = currentRight.point
            currentCorner.co = (intersection[0], intersection[1], thisLocation[2])
            currentCornerCurb.co = (intersection[0], intersection[1], thisLocation[2] + STREET_CURB_HEIGHT)

            shiftRight(currentRight, STREET_SIDE_WALK_WIDTH)
            shiftLeft(lastLeft, STREET_SIDE_WALK_WIDTH)
            intersection = currentRight.intersect(lastLeft)
            if intersection == None:
                intersection = currentRight.point
            currentCornerSideWalk.co = (intersection[0], intersection[1], thisLocation[2] + STREET_CURB_HEIGHT)
            
            #Calculating beveling coefficient
            theta = abs(math.atan2(currentRight.vector[1], currentRight.vector[0]) - math.atan2(lastLeft.vector[1], lastLeft.vector[0]))
            if theta > math.pi: 
                theta = 2 * math.pi - theta
            alpha = (1 - theta / math.pi) ** 2
            
            verticleEdges.update({(currentCorner.index, currentCornerCurb.index):alpha})
            verticleEdges.update({(currentCornerCurb.index, currentCorner.index):alpha})
            
        for i in range(len(listOfConnections) - 1):
            edgePlaneDict.update({(vertex, listOfConnections[i]):(corners[i], cornersCurb[i], cornersSideWalk[i], corners[i + 1], cornersCurb[i + 1], cornersSideWalk[i + 1])})
        edgePlaneDict.update({(vertex, listOfConnections[-1]):(corners[-1], cornersCurb[-1], cornersSideWalk[-1], corners[0], cornersCurb[0], cornersSideWalk[0])})
        if len(corners) > 2:
            createPolygon(streetObject.data, corners)

print("Finished creating vertex-to-vertex map. Bridging streets.")

edgeLoopDict = {}
loopCandidate = set()
edgeDone = set()
for edgeKey in list(edgePlaneDict.keys()):
    if edgeKey not in edgeDone:
        otherKey = (edgeKey[1], edgeKey[0])
        
        #street
        vertexList = []
        vertexList.append(edgePlaneDict[edgeKey][int(len(edgePlaneDict[edgeKey]) / 2)])
        vertexList.append(edgePlaneDict[edgeKey][0])
        vertexList.append(edgePlaneDict[otherKey][int(len(edgePlaneDict[edgeKey]) / 2)])
        vertexList.append(edgePlaneDict[otherKey][0])
        createPolygon(streetObject.data, vertexList)
        
        for i in range(int(len(edgePlaneDict[edgeKey]) / 2 - 1)):
            #right side
            vertexList = []
            vertexList.append(edgePlaneDict[edgeKey][i])
            vertexList.append(edgePlaneDict[edgeKey][i + 1])
            vertexList.append(edgePlaneDict[otherKey][int(len(edgePlaneDict[edgeKey]) / 2) + i + 1])
            vertexList.append(edgePlaneDict[otherKey][int(len(edgePlaneDict[edgeKey]) / 2) + i])
            createPolygon(streetObject.data, vertexList)
        
            #left side
            vertexList = []
            vertexList.append(edgePlaneDict[edgeKey][int(len(edgePlaneDict[edgeKey]) / 2) + i + 1])
            vertexList.append(edgePlaneDict[edgeKey][int(len(edgePlaneDict[edgeKey]) / 2) + i])
            vertexList.append(edgePlaneDict[otherKey][i])
            vertexList.append(edgePlaneDict[otherKey][i + 1])
            createPolygon(streetObject.data, vertexList)
        
        vertex1 = edgePlaneDict[edgeKey][int(len(edgePlaneDict[edgeKey]) / 2) - 1]
        vertex2 = edgePlaneDict[otherKey][-1]
        vertex3 = edgePlaneDict[otherKey][int(len(edgePlaneDict[edgeKey]) / 2) - 1]
        vertex4 = edgePlaneDict[edgeKey][-1]
        appendDict(edgeLoopDict, vertex1, vertex2)
        appendDict(edgeLoopDict, vertex2, vertex1)
        appendDict(edgeLoopDict, vertex3, vertex4)
        appendDict(edgeLoopDict, vertex4, vertex3)
        loopCandidate.add(vertex1)
        loopCandidate.add(vertex2)
        loopCandidate.add(vertex3)
        loopCandidate.add(vertex4)
        edgeDone.add(edgeKey)
        edgeDone.add(otherKey)

print("Finished bridging streets. Filling single corners.")

for singleCorner in singleCorners:
    endVertices = edgePlaneDict[singleCorner]
    vertexList = [endVertices[4], endVertices[1], endVertices[0], endVertices[3]];
    createPolygon(streetObject.data, vertexList)
    appendDict(edgeLoopDict, endVertices[4], endVertices[1])
    appendDict(edgeLoopDict, endVertices[1], endVertices[4])
    appendDict(edgeLoopDict, endVertices[1], endVertices[2])
    appendDict(edgeLoopDict, endVertices[2], endVertices[1])
    appendDict(edgeLoopDict, endVertices[4], endVertices[5])
    appendDict(edgeLoopDict, endVertices[5], endVertices[4])
    loopCandidate.add(endVertices[1])
    loopCandidate.add(endVertices[4])
    
print("Finish filling single corners. Filling loops.")

minY = streetObject.data.vertices[list(loopCandidate)[0]].co[1]
toRemove = list(loopCandidate)[0]
for vertex in list(loopCandidate):
    currentY = streetObject.data.vertices[vertex].co[1]
    if currentY < minY:
        minY = currentY
        toRemove = vertex
loopCandidate.remove(toRemove)
if FILL_LOOP:
    while len(loopCandidate) > 0:
        loopVertices = []
        startVertex = list(loopCandidate)[0]
        pointer = edgeLoopDict[startVertex][0]
        if pointer in loopCandidate:
            loopCandidate.remove(pointer)
        loopCandidate.remove(startVertex)
        loopVertices.append(startVertex)
        loopVertices.append(pointer)
        while pointer != startVertex:
            carryOn = False
            for connectVertex in edgeLoopDict[pointer]:
                if connectVertex in loopCandidate:
                    carryOn = True
                    pointer = connectVertex
                    loopCandidate.remove(pointer)
                    loopVertices.append(pointer)
                    break
            if not carryOn: break
        if pointer in edgeLoopDict[startVertex]:
            if len(loopVertices) > 2:
                firstThree = [streetObject.data.vertices[vertex].co for vertex in loopVertices[:3]]
                vector1 = tuple([firstThree[1][i] - firstThree[0][i] for i in range(2)])
                vector2 = tuple([firstThree[2][i] - firstThree[1][i] for i in range(2)])
                crossProduct = vector1[0] * vector2[1] - vector1[1] * vector2[0]
                if crossProduct < 0:
                    loopVertices.reverse()
                createPolygon(streetObject.data, loopVertices)

print("Finish filling loops. Beveling curb corners.")

streetObject.data.update(calc_edges=True, calc_tessface=True)

#Bevel the corners
bpy.context.scene.objects.active = streetObject
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.normals_make_consistent()
oldMode = tuple(bpy.context.tool_settings.mesh_select_mode)
bpy.context.tool_settings.mesh_select_mode = (False, True, False)
bMesh = bmesh.from_edit_mesh(bpy.context.object.data)

edgeToBevel = []
for edge in bMesh.edges:
    edgeKey = (edge.verts[0].index, edge.verts[1].index)
    if edgeKey in verticleEdges:
        alpha = verticleEdges[(edge.verts[0].index, edge.verts[1].index)]
        edgeToBevel.append((edge, alpha))

counter = 0
for element in edgeToBevel:
    counter = counter + 1
    edge = element[0]
    alpha = element[1]
    if counter % 100 == 0:
        print(str(counter) + "/" + str(len(edgeToBevel)) + " finished")
    #bmesh.ops.bevel(bMesh, geom=[edge.verts[0], edge.verts[1], edge], offset = CORNER_BEVEL_OFFSET * alpha, segments = int(CORNER_BEVEL_SEGMENTS * alpha))
    #bmesh.update_edit_mesh(bMesh, True)
    bpy.ops.mesh.select_all(action='DESELECT')
    edge.select = True
    bpy.ops.mesh.bevel(offset = CORNER_BEVEL_OFFSET * alpha, segments = int(CORNER_BEVEL_SEGMENTS * alpha))

bpy.context.tool_settings.mesh_select_mode = oldMode
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.remove_doubles(threshold=.01)
bpy.ops.mesh.select_all(action='DESELECT')
bpy.ops.object.mode_set(mode="OBJECT")
bpy.context.scene.objects.active = streetSkeleton

print("Finished beveling corners")

print("Done!!!")