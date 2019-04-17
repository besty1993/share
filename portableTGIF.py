import cv2
import numpy as np
import os.path
import traceback
import matplotlib.pyplot as plt

import readFlowFile

def GetMagField(totalFlow) :
    newflow = totalFlow  # newflow = flow[90:650, 290:]
    tdField = newflow[:,:,0]*newflow[:,:,0]+newflow[:,:,1]*newflow[:,:,1]   ### x^2+y^2
    tdField = np.sqrt(tdField)                                              ###sqrt(x^2+y^2)
    return(tdField)


try:
    tgifProcessing = "C:/Project/COG/ABMCTS-OF/tgifProcessing.txt"
    tgifDone = "C:/Project/COG/ABMCTS-OF/tgifDone.txt"
    tgifSave = "C:/Project/COG/ABMCTS-OF/tgifSave.txt"

    if (os.path.isfile(tgifDone)):
        os.remove(tgifDone)

    file = open(tgifProcessing, "a+")
    file.write("aaaa")
    file.close()


    dirName = ""
    totalMagField = []
    temp = []  # temporal : List of frame magnitude, spatial : Field of pixel magnitude
    pixel = 0
    maxMag = np.inf
    aveMag = np.inf

    for i in range(0, 400):
        
        fileName = "C:/Project/COG/ABMCTS-OF/flow_" + str(i) + "_" + str(i+1) + ".flo"
        if os.path.isfile(fileName):
        #### Main Calculation Starts
            flowField = readFlowFile.read(fileName)
        # Get magnitude of each vector and apply filter
            magField = GetMagField(flowField)
            if (temp == []):
                temp = magField
            else:
            # Sum up all the magnitude fields into one field.
                temp = temp + magField
            # os.remove(fileName)
            # if (os.path.isfile(str(i+1)+".png")):
            #     os.remove(str(i+1)+".png")

# Getting total displacement by adding all the magnitudes.
    # print (temp)
    totalDisplacement = float(sum(sum(temp)))
    temp = np.array(temp)
# probability is pixel magnitude divided by total displacement
    probability = temp / totalDisplacement

### Entropy Calculation
    entropyField = 0 - (probability * np.log2(probability))
    entropyField = np.nan_to_num(entropyField)
    entropy = sum(sum(entropyField))
###################

    TGIF = entropy * totalDisplacement / np.log2(temp.size) / temp.size
    print(TGIF)


    file = open(tgifDone, "a+")
    file.write(str(TGIF))
    file.close()

    if (os.path.isfile(tgifProcessing)):
        os.remove(tgifProcessing)


except:
    traceback.print_exc()
