# Unity Notebook

Welcome to Unity Notebook! This project brings the interactive coding experience of Jupyter Notebook to Unity, allowing you to use C# to create interactive projects within the Unity editor.

## Getting Started

To add Unity Notebook to an existing Unity project, follow these steps:

1. Open your Unity project.
2. In the Unity editor, go to `Window > Package Manager`
3. Click the `+` button in the top left corner and select `Add package from git URL...`
4. In the URL field enter `https://github.com/mstevenson/UnityNotebook.git`
5. Click the Add button.
6. The Unity Notebook package will now be installed in your Unity project.

To begin using the tool, select the menu `Window > Notebook` to dislay its main window.

* To create a new Notebook, click the `Create Notebook` button and select the name and location for your Notebook asset file. The asset will be created in Jupyter's standard `ipynb` file format.
* To open an existing Notebook, either click `Open Notebook` and navigate to the Notebook file location within your Unity project, or click the `Notebooks` popup menu to select from a list of discovered Notebooks within the current project.

You can also double-click on a Notebook asset in the Unity Project view to display it in the Notebook window.

## Using Notebook

Unity Notebook is designed to function very similarly to the original Jupyter Notebook, with a few key differences to account for the integration with the Unity Editor.

One of the main goals of Unity Notebook is to provide a familiar and intuitive coding experience for developers who are already familiar with Jupyter Notebooks, so it adopts most of Jupyter's conventions such as storing blocks code in cells that can be rearranged and executed in any order while retaining local variable state, and adding in-line documentation in markdown-formatted cells. It also replicates Jupyter's keyboard shortcuts where applicable, such as `Shift + Enter` to run a cell and move to the next one.

If you are unfamiliar with Jupyter Notebook it is recommended to peruse its documentation: https://jupyter-notebook.readthedocs.io/en/latest/

### Code Blocks

C# code blocks offer some notable features:

- Code blocks have access to the `UnityEngine` and `UnityEditor` namespaces.
- Cell outputs can display primitive types such as `float` and `string`, as well as many Unity types, such as `GameObject`, `Material`, and `Color`.
    - The `return` statement will display a resulting value in the cell's output.
    - The `yield` statement will turn the block into a coroutine and display each yielded value in the cell's output.
    - A special static method `Show(object value)` can be called to display a value in the cell's output.
- Code blocks can be executed either during edit mode or play mode. Note that play mode scene changes will not persist, and Notebooks can not be executed in builds.
- All local variable state will be preserved across runs, except for coroutine blocks that include one or more `yield` statements.

### Execution & State Management

Unity Notebook offers a simple toolbar for managing the storage and execution of Notebooks:

* `Run` – executes all code cells sequentially.
* `Clear` – removes all displayed output from all cells.
* `Restart` – clears all stored local variable state that was generated during previous code cell executions.
* `Save` – saves in-Editor Notebook asset changes back to the Notebook's `ipynb` file. Note that changes to the Unity asset will be automatically saved during editing, but `Save` must be explicitely invoked to modify the asset file on disk.
* `Revert` – restore the in-Editor Notebook's state from the `ipynb` asset file on disk.
* `Edit` – open the Notebook's `ipynb` file in an external program, such as Visual Studio Code.

Additionally, individual cells can be executed by pressing their `▶` button, and their individual output can be cleared by pressing the `✕` button.

## Resources

* To view and edit C# Notebooks in Visual Studio Code, install [Microsoft's Polyglot Notebooks extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
* For more information on using Jupyter Notebook, see the [Jupyter documentation](https://jupyter-notebook.readthedocs.io/en/latest/)
* To learn about Microsoft's Roslyn and its interactive execution engine, see the [.Net Interactive Documentation](https://github.com/dotnet/interactive/blob/main/docs/README.md)