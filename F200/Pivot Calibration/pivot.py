import numpy as np
import sys

def pivotCalibration(transforms):
    A = np.empty(shape=[0, 6])
    B = np.empty(shape=[0, 1])
    for F in transforms:
        subA = np.concatenate((F[:3,:3], [[-1., 0., 0.], [0., -1., 0.], [0., 0., -1.]]), axis=1)
        A = np.concatenate((A, subA))
        subB = -F[:3, 3].reshape(3,1)
        B = np.concatenate((B, subB))
    answer = np.linalg.lstsq(A, B)[0]
    pointTip = np.array([answer[0,0]*1000, answer[1,0]*1000, answer[2,0]*1000, 1]).reshape(4,1)
    pointPivot = np.array([answer[3,0]*1000, answer[4,0]*1000, answer[5,0]*1000, 1]).reshape(4,1)
    return pointTip, pointPivot


def getResidues(transforms, pointTip, pointPivot, verbose=False):
    for t in transforms:
        residue = pointPivot - t.dot(pointTip)
        if verbose:
            print(np.linalg.norm(residue))

def main(filename="zzkMarker0.txt"):
    f = open(filename)
    transforms = list()
    l = 0
    for line in f:
        if l == 0:
            F = np.empty(shape=[4, 4])
        words = line.split()
        F[l, 0] = float(words[0])
        F[l, 1] = float(words[1])
        F[l, 2] = float(words[2])
        F[l, 3] = float(words[3])
        l = l + 1
        if l == 4:
            transforms.append(F)
            l = 0

    # print transforms

    pointTip, pointPivot = pivotCalibration(transforms)

    print ("Point Position with respect to camera is: ")
    print (pointPivot)
    print ("Point Position with respect to tool marker is: ")
    print (pointTip)
    # write the pointTip to a txt file
    filename = "pointTipNeedle.txt"
    # np.savetxt(filename, pointTip.reshape(1, pointTip.shape[0]), fmt = "%f")
    np.savetxt(filename, pointTip, fmt = "%f")

if __name__ == "__main__":
    # stuff only to run when not called via 'import' here
    if len(sys.argv) != 2:
        main()
    else:
        main(sys.argv[1])
