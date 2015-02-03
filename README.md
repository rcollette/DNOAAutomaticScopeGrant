DotNetOpenAuth Automatic Scope Grant Example
==============

This project is an example of using OAuth2 to perform a login where the requested scope is automatically granted after the user performs login.  The use case might be where you are hosing a custom OAuth provider and the third party is well known and trusted to access basic user profile data.

Currently, there are a couple bugs in the DotNetOpenAuth master branch where there are incorrect assembly references.  These have been resolved in the DotNetOpenAuth development branch.   [Here](https://github.com/DotNetOpenAuth/DotNetOpenAuth/commit/a409f200f621915dcd2c4b50b406213b3b6d6359) is the merge fix.   This project will be updated to use a released version of DotNetOpenAuth once the nuget packages have been updated. 
