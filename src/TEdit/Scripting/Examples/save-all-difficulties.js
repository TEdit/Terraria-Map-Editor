/// <reference path="tedit-api.d.ts" />
// Save All Difficulties - Save the current world as all 4 difficulties
// Parses the world title and file name to swap the difficulty word.
//
// Expected format:
//   World Name: "Shimmering Skies - Journey 1.7"
//   File Name:  "ShimmeringSkies-Journey-1.7.wld"
//
// Produces 4 files in the same folder as the source file:
//   ShimmeringSkies-Classic-1.7.wld  (GameMode 0)
//   ShimmeringSkies-Expert-1.7.wld   (GameMode 1)
//   ShimmeringSkies-Master-1.7.wld   (GameMode 2)
//   ShimmeringSkies-Journey-1.7.wld  (GameMode 3)
//
// If no path separators in the file name, saves to the default worlds folder.

var difficulties = [
    { mode: 0, name: "Classic" },
    { mode: 1, name: "Expert" },
    { mode: 2, name: "Master" },
    { mode: 3, name: "Journey" }
];

var originalTitle = world.title;
var originalMode = world.gameMode;
var originalPath = tools.getFilePath();

if (!originalPath) {
    log.error("No file path set. Save the world first.");
} else {
    // Extract directory and file name parts
    var lastSlash = originalPath.lastIndexOf("\\");
    if (lastSlash < 0) lastSlash = originalPath.lastIndexOf("/");
    var dir = lastSlash >= 0 ? originalPath.substring(0, lastSlash + 1) : "";
    var fileName = lastSlash >= 0 ? originalPath.substring(lastSlash + 1) : originalPath;
    var baseName = fileName.replace(/\.wld$/i, "");

    // Find which difficulty word is in the current title and file name
    var currentDiffName = "";
    for (var i = 0; i < difficulties.length; i++) {
        if (originalTitle.indexOf(difficulties[i].name) >= 0) {
            currentDiffName = difficulties[i].name;
            break;
        }
    }

    if (!currentDiffName) {
        log.warn("Could not detect difficulty in world title: " + originalTitle);
        log.warn("Will append difficulty to title and file name instead.");
    }

    log.print("World title:  " + originalTitle);
    log.print("File:         " + fileName);
    log.print("Worlds folder: " + tools.getWorldsFolder());
    log.print("Detected difficulty: " + (currentDiffName || "(none)"));
    log.print("---");

    var saved = 0;
    for (var d = 0; d < difficulties.length; d++) {
        var diff = difficulties[d];
        log.progress((d + 0.1) / difficulties.length);

        // Build new title
        var newTitle;
        if (currentDiffName) {
            newTitle = originalTitle.replace(currentDiffName, diff.name);
        } else {
            newTitle = originalTitle + " - " + diff.name;
        }

        // Build new file name
        var newBaseName;
        if (currentDiffName) {
            newBaseName = baseName.replace(currentDiffName, diff.name);
        } else {
            newBaseName = baseName + "-" + diff.name;
        }
        var newPath = dir + newBaseName + ".wld";

        // Apply changes
        world.title = newTitle;
        world.gameMode = diff.mode;

        log.print("Saving " + diff.name + ": " + newTitle);
        log.print("  -> " + newPath);

        var ok = tools.saveAs(newPath);
        if (ok) {
            saved++;
        } else {
            log.error("Failed to save: " + newPath);
        }

        log.progress((d + 1) / difficulties.length);
    }

    // Restore original state
    world.title = originalTitle;
    world.gameMode = originalMode;
    tools.setFilePath(originalPath);

    log.print("---");
    log.print("Done! Saved " + saved + "/" + difficulties.length + " files.");
}
