# Boulder Trap Actuator Bug

Related issues: #1830

## Problem

Boulder trap actuators are deleted when a world is loaded in TEdit. Lava traps and the boulder above deadman's chests do not have this issue.

## Investigation Needed

- Determine why specifically boulder trap actuators are lost during load
- Check if this is a read/parse issue (actuator flag not read) or a tile-matching issue (boulder trap tiles not recognized as having actuators)
- Compare boulder trap tile data with lava trap tile data to find the difference
- May be related to how TEdit handles multi-tile sprites with actuators
