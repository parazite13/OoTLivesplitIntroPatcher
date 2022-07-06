# OoTLivesplitIntroPatcher

Starting from the 07/06/2022 the timing method of Ocarina of Time speedrun has been changed to allow the intro to be skipped.
This project is used to patch livesplit split file to remove the time of the intro (2'52) and thus preserve the history of the runs

# Usage 
> OoTLivesplitIntroPatcher.exe --split-path dummy/path/split.lss

It won't override your old split file, it will automatically created a new split file with the suffix *patched*.
If you want to specify the output file you can do so by using the *--output-path* argument