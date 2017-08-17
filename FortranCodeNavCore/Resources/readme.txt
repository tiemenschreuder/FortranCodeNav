=== About ===

Fortran CodeNav is a Visual Studio Extension which helps you to navigate through 
Fortran 90 code. It contains several parsers to generate a code structure 
representation which is used to provide jump-to and search functionality with limited 
context-awareness. Additionally it provides scope-aware code completion (Intellisense). 

It currently supports the following code elements/blocks:
- modules
- subroutines
- functions
- interfaces
- types

=== Usage ===

Fortran CodeNav can be completely controlled using keyboard shortcuts. Commands typically 
only work when you have a f90 document view open in Visual Studio.

The definition of 'member' is: modules, subroutines, functions, interfaces and types. 

      Commands:
      Ctrl-Q: Jump to declaration of member under (text) cursor
      Alt-Q: List usages of element under (text) cursor
      
      Ctrl-Space: Auto-complete statement, or show completion hints in list (Intellisense)
      
      Ctrl-Alt-Q: Search all code elements in solution
      Ctrl-Shift-Q: Search/list all members in current file
      Ctrl-Shift-Alt-Q: Search all files in solution
      
      Alt-Shift-Q: Synchronize solution explorer with currently open file

The search dialog uses two types of text searches. It uses a case-insenstive 'starts-with' search
and a second pattern search which takes the casing of the search term into account (see section 
'Pattern Search'). Any member matching either search method is shown as search result.

For ReSharper users: Fortran CodeNav does not use ReSharper, so a ReSharper installation is not 
required. The shortcuts however were chosen not to conflict with the ReSharper IDEA keymapping 
for those users who have both installed.


=== Change Keyboard Shortcuts ===

Should you wish to change the shortcut keys used by FortranCodeNav, you can do so in Visual Studio, 
under Tools->Options and then Environment->Keyboard. In the search box, type in FortranCodeNav and 
the applicable commands and their current shortcuts are listed.


=== Pattern Search ===

Pattern search is a method to quickly filter the results without typing out the entire search term. 
In the pattern search all uppercase letters are considered to be followed by any number of other 
letters while all lowercase letters must be an exact match.

So the search term 'HL' would match 'hello' whereas the search term 'hl' would not. Expressed as a 
(case insensitive) wildcard pattern:
hl = 'hl'
HL = 'h*l*'

The pattern search is useful to quickly find members by their acronym:
Sobek = SBK
boundaryconditions = BouCon
wave_to_flow_status = WToFS

Note for ReSharper users: unlike ReSharper, Fortran CodeNav does not take the casing of the member 
names into account.