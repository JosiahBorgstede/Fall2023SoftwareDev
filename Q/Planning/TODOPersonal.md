## TODO:
- map is still tied to the rules of Q
    - can we move this to the IActions? this would localize the rules.

- GetScore has a required order that is not documented but must be enforced by
referee. Needs to not have order or be documented.


- changes to player data should probably be localized to the game state.
    - perhaps do this for score?

- Create/clean PublicGameInfo(public to everyone), and split with
PlayerTurnInfo
    - Purpose statements

- clean up purpose statements in general
    - Biggest offenders are fields and properties
    - second worst is private methods
    - should add links to interface if applicable

- Placement has an entire file, not needed

- Make more of the info holders renderable
    - make the number of tiles referee has rendered
    - make a full state renderable

- Unit Test Json converters, and potential rendering if possible

- try/catch in referee could be abstracted, perhaps add timeout functionality
there

## Places that need purpose statements:
- GameState:
  - 
