# Q Game

This repository contains a work-in-progress implementation of the Q game. The
game will eventually be playable via a client-server architecture.

To build the project, run:

    cd Common
    dotnet build

If you see the error message about "disk quota exceeded" or some IO issue, the
problem is not our project; it's the Khoury server. Check what's taking up the
most space and delete other files (e.g. old repositories, cache directories)
until there is enough space that the project builds. We found that it's very
easy to hit the Khoury server disk quota.

## Code Structure

The directory `Common` contains the code for the game map:

- `map.cs` defines the `Map` class for the game board.
- `Coordinate.cs` defines the `Coordinate` class to describe map locations
- `Tile.cs` defines the `Tile` class for the game tiles

The Map class uses the `Coordinate` and `Tile` classes in its API.

The directory `Other` contains the tests.

## Testing

To test the program, run:

    ./xtest
