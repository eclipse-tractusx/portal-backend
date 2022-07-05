## Copyright and License Header

Where possible, all source code, property files, and metadata files (including application, test, and generated source code as well as other types of files such as XML, HTML, etc.) must include a header with appropriate copyright and license notices.

## Use the 'File Header Comment' VS Code extension

It's recommended to use the [File Header Comment](https://marketplace.visualstudio.com/items?itemName=doi.fileheadercomment) VS Code extension because it allows us to share the header within the team by the .vscode/settings.json.

Install the extension and assign some [keyboard shortcut](https://code.visualstudio.com/docs/getstarted/keybindings#_keyboard-shortcuts-editor) to the extension insertFileHeaderCommentOther, Ctrl+Alt+I for instance.

Every time you create a new file or edit a file which doesn't yet have a header use the keyboard shortcut to insert the according header template in the file.

Currently the following templates are available:
* cx_header_default
* cx_header_with_#