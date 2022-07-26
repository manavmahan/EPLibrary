import sys
from shapely.geometry import Point, Polygon
import shapely
from shapely.ops import polygonize

# strT = "13.06,0;13.06,6.53;0,6.53;0,0:13.06,0;13.06,32.65;8.56,28.15;8.56,4.5"
# strT = "13.06,0;13.06,6.53;0,6.53;0,0:13.06,8;13.06,32.65;8.56,28.15;8.56,8"
# strT = "15.38000,0.00000,.000000;15.38000,15.38000,.000000;7.69000,15.38000,.000000;7.69000,30.76000,.000000;15.38000,30.76000,.000000;15.38000,46.14000,.000000;0.00000,46.14000,.000000;0.00000,0.00000,.000000"
# strT += ":" + "10.88000,4.50000,.000000;10.88000,10.88000,.000000;3.19000,10.88000,.000000;3.19000,35.26000,.000000;10.88000,35.26000,.000000;10.88000,41.64000,.000000;4.50000,41.64000,.000000;4.50000,4.50000,.000000"
strT = '41.84231,29.11512;26.67463,41.84231;13.94744,26.67463;29.11512,13.94744:49.42615,22.75152;19.09079,48.20591;19.64545,41.86617;43.08641,22.19686'

strT = sys.argv[1]

def get_string(point):
    return f'{point.x:.5f},{point.y:.5f}'

def is_collinear(p1, p2, p3):
    a = p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y)
    return abs(a) <= 1E-04

def add_to_loop(points, p):
    if (len(points) == 0):
        points += [p]
        return

    if p in points:
        return

    if len(points) > 1:
        p1 = points[-2]
        p2 = points[-1]
        if (is_collinear(p1, p2, p)):
            del points[-1]

    points += [p]

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

if (len(sys.argv) > 2):
    import matplotlib
    matplotlib.use('Agg')
    # %matplotlib inline
    import matplotlib.pyplot as plt

    plt.style.use('ggplot')
    colors = plt.rcParams['axes.prop_cycle'].by_key()['color']

    fig, ax = plt.subplots(figsize=(5,5))
    for i, str1 in enumerate(polygons + [intersect]):
        px = str1.exterior.coords.xy[0]
        py = str1.exterior.coords.xy[1]
        ax.plot(px, py, color = colors[i])
        plt.text(-8, i*5, f'S-{i}', size = 16, color = colors[i])

    ax.set_xlim([-10, 60])
    ax.set_ylim([-10, 60])

    plt.savefig(f"{sys.argv[2]}.png")

points = []
st = ''
x = intersect.exterior.coords.xy[0]
y = intersect.exterior.coords.xy[1]
for i in range(len(x)):
    p = Point(round(x[i], 5), round(y[i], 5))
    add_to_loop(points, p)

print (';'.join([get_string(x) for x in points]))