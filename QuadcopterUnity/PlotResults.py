import matplotlib.pyplot as plt
import numpy as np
import sys

if len(sys.argv) < 2:
    print("Please specify a file name")
    quit()

csvName = sys.argv[1]
data = np.genfromtxt(csvName, delimiter=',', skip_header=1)
columns = data.shape[1]
for col in range(1, columns):
    timeSeries = data[:,0]
    currentColumn = data[:,col]
    plt.plot(timeSeries, currentColumn)
plt.show()
