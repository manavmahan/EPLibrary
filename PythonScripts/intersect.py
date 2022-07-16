import matplotlib
matplotlib.use('Agg')
# %matplotlib inline

from shapely.geometry import Point, Polygon, GeometryCollection
import shapely

import  matplotlib.pyplot as plt
import sys

# strT = "13.06,0;13.06,6.53;0,6.53;0,0:13.06,0;13.06,32.65;8.56,28.15;8.56,4.5"
# strT = "13.06,0;13.06,6.53;0,6.53;0,0:13.06,8;13.06,32.65;8.56,28.15;8.56,8"
# strT = "15.38000,0.00000,.000000;15.38000,15.38000,.000000;7.69000,15.38000,.000000;7.69000,30.76000,.000000;15.38000,30.76000,.000000;15.38000,46.14000,.000000;0.00000,46.14000,.000000;0.00000,0.00000,.000000"
# strT += ":" + "10.88000,4.50000,.000000;10.88000,10.88000,.000000;3.19000,10.88000,.000000;3.19000,35.26000,.000000;10.88000,35.26000,.000000;10.88000,41.64000,.000000;4.50000,41.64000,.000000;4.50000,4.50000,.000000"

strT = sys.argv[1]

polygons = []
for pol in strT.split(':'):
    points = []
    ps = pol.split(';')
    for p in ps:
        coor = p.split(',')
        points += [(float(coor[0]), float(coor[1]))]
    polygons += [(Polygon(points))]

if len(polygons) < 2:
    quit()

p1 = polygons[0]
p2 = polygons[1]
intersect = p1.intersection(p2)

for i in range(2, len(polygons)):
    p = polygons[i]
    intersect = intersect.intersection(p)

if (intersect.type != 'Polygon'):
    quit()
    
if (len(intersect.exterior.coords.xy[0]) == 0):
    quit()
                    
# plt.style.use('ggplot')
# colors = plt.rcParams['axes.prop_cycle'].by_key()['color']

# fig, ax = plt.subplots(figsize=(5,5))
# for i, str1 in enumerate(polygons + [intersect]):
#     px = str1.exterior.coords.xy[0]
#     py = str1.exterior.coords.xy[1]
#     ax.plot(px, py, color = colors[i])
#     plt.text(-8, i*5, f'S-{i}', size = 16, color = colors[i])

# ax.set_xlim([-10, 60])
# ax.set_ylim([-10, 60])

# plt.savefig("1.png")

st = ''
x = intersect.exterior.coords.xy[0]
y = intersect.exterior.coords.xy[1]
for i in range(len(x)):
    st += f'{x[i]},{y[i]};'
    
print (st)