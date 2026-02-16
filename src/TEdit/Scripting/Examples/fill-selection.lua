-- Fill Selection - Fill the current selection with a specified tile type
-- Edit the tileType value below to customize

local tileType = 1  -- Stone

if not selection.isActive() then
    log.warn("No selection active. Please make a selection first.")
    return
end

local count = 0

batch.forEachInSelection(function(x, y)
    tile.setType(x, y, tileType)
    count = count + 1
end)

local name = metadata.tileName(tileType)
log.print("Filled " .. count .. " tiles with " .. name)
