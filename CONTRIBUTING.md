# Contributing

## Style

- Always have trailing line endings on files. This is a setting you can enable in most IDEs
- For markdown obey the style guides of a quality linter such as `DavidAnson.vscode-markdownlint`
- For Source files, try avoid nesting by using early returns under negative checks instead of branches under positive checks. Example:

  ```cs
  //DO not
  If (thing)
  {
    //do stuff here
  }

  //Do
  {
    If (!thing)
      return;

    //do stuff here
  }
  ```

## Commits / Pull requests

- Be descriptive of what your commits do.
- Follow [Convetional Commits](https://www.conventionalcommits.org/en/v1.0.0/) to help keep track of what you are changing both for yourself and others that wish to read your code
- **Do Not** commit changes to packages.lock.json
- **Avoid** commiting changes to .gitignore since this will effect everyone's instance if merged. Use `git rm <file>` instead to ask git to stop tracking a file
