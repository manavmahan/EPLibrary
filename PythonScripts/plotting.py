import matplotlib

import  matplotlib.pyplot as plt
matplotlib.use('Agg')
import sys

strT = "15.02,0;15.02,15.02;7.51,15.02;7.51,30.04;15.02,30.04;15.02,37.55;0,37.55;0,0"

strT = sys.argv[1]

strs = strT.split(':')

plt.style.use('ggplot')
colors = plt.rcParams['axes.prop_cycle'].by_key()['color']

fig, ax = plt.subplots(figsize=(5,5))
for i, str1 in enumerate(strs):
    px = [float(x.split(',') [0]) for x in str1.split(';')]
    py = [float(x.split(',') [1]) for x in str1.split(';')]
    px = px + [px[0]]
    py = py + [py[0]]
    ax.plot(px, py, color = colors[i])
    plt.text(-8, i*5, f'S-{i}', size = 16, color = colors[i])

ax.set_xlim([-10, 60])
ax.set_ylim([-10, 60])

plt.savefig("C:/Users/manav/Desktop/1.png")