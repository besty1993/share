import numpy as np
import argparse
import os.path
from scipy.misc import toimage
import pandas as pd
import scipy.stats as st
import matplotlib

# parser = argparse.ArgumentParser()
# parser.add_argument("-x","--x", help="x value", default=100.0, type=float)
# parser.add_argument("-k","--k", help="K value", default=33.61, type=float)
# parser.add_argument("-l","--los", help="loc value", default=12.76, type=float)
# parser.add_argument("-s","--scale", help="Scale", default=2.41, type=float)

# args = parser.parse_args()
# print(st.exponnorm.cdf([args.x],args.k,args.los,args.scale)[0])

# 0118version
parser = argparse.ArgumentParser()
parser.add_argument("-x","--x", help="x value", default=100.0, type=float)
parser.add_argument("-k","--k", help="K value", default=1.15, type=float)
parser.add_argument("-l","--los", help="loc value", default=0.05, type=float)
parser.add_argument("-s","--scale", help="Scale", default=1.34, type=float)

args = parser.parse_args()
print(st.foldcauchy.cdf([args.x],args.k,args.los,args.scale)[0])

