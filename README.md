# fastAStarPathfinder
My implementation of the A Star Algorithm for pathfinding

To Use: Add the included aStarPathfinder class to your object trying to pathfind, and just call the Pathfind function to get a array of points to pathfind between.
It takes a 2d int array as the input for the map data, where 0 represents a passable tile, and 1 (or any other number) is impassible

I have included a Unity Engine version, and a version without Unity.

The non unity version includes a custom Vec2Int struct, equivalent to Unity's Vector2Int, as it is important to how this program works.
