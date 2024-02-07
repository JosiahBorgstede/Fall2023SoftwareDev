## Todo



## Completed
1. Converting between external players and internal player data is a mess  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/24b830f1eb842efe97e6ae2dbe7b8cd9cebc0796)  

2. Determine how to manage the concept of round, and who is responsible for it  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/9231e49d5d920de21ea59988cb7e30dc259cd855)  

3. player name(), setup(), and win() are called in a way where if it throws, the
referee will crash  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/cb5699516e7fdacf1212a03e5b5876eddca8ffee)  

4. Change scoring to allow for changable values.  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/b9d3c7f4e27bb53a40902c517ac72e3c2eded732)  

5. Setting up the game state and calling setup() on all the players is messy  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/5bca914fa54b9c6a4c25eaec17516237219224a9)  

6. the game state builder is required to be used, makes it hard to create a
state for testing.  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/6b52c43181edcb784b0f2ce2ceaeb846d5e6ee0d)  

7. map should be the one to find all connected tiles in a row or column  
8. map should be able to add multiple tiles at once, function of map rather
than anywhere else  
[Both were done here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/cc849a68afe9155e0019b83c0041bb6a25fbb1d8)  

9. Player and ReadOnlyPlayer contain the same data and having the two types
   leads to redundant serialization code  
[Here](https://github.khoury.northeastern.edu/CS4500-F23/whimsical-mongooses/commit/ab3090d7844db7476c2fcfe1c0ecd9e364da4489)

