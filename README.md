# WinCursorChanger
Easy to use .NET library for replacing cursors on Windows with any .cur file as well as restore the users default cursors.

## About the Project
- I wrote this library to use alongside my [GTACursor](https://github.com/MaxBranvall/GTACursor) application. *Talk about over-engineering*.
- The library is completely independant of that application and allows any cursor defined in the Cursors enum to be swapped with a specified .cur file.
    - [This link](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setsystemcursor) provides the values I used in the enum.
    - [This page](https://docs.microsoft.com/en-us/windows/win32/menurc/cursors) provides an overview of Cursors on Windows and provides in-depth information
    as well as methods that can be invoked from the win32 API relating to cursors.
- Special thanks to [pinvoke.net](https://www.pinvoke.net/) for being a fantastic resource while working with p/invoke!

## Using This Library
- Currently, I am not planning on putting this library on Nuget.
- Instead, clone the repo to your machine and reference the library.

## Contributing
- Contributions are welcome! I am positive there are many changes that can be made to possibly improve performance and 
follow best practices.
- In case you are new to contributing, here are the basic steps to doing so:
  - Fork this repo.
  - Create and checkout a branch in your local repo.
  - Write and commit your changes.
  - Submit a PR to this repo!

# Thanks for checking it out! If you have any suggestions, questions, comments, etc. feel free to open an issue or contact me!
