# Steam Cloud Saves in World Browser

Related issues: #1274, #948

## Overview

Display Steam cloud save files in the world browser alongside local saves.

## Requirements

### Display
- Steam cloud world files appear in the world browser list
- Cloud icon indicator to distinguish from local saves
- Sort/group: cloud saves can be intermixed or grouped separately (TBD)

### Interaction
- **View-only** for initial release — cloud saves cannot be directly edited
- Opening a cloud save shows world info/preview but does not allow saving changes back to cloud
- Tooltip on cloud saves: "This save is stored in Steam Cloud. Move it to a local folder to edit."

### Future Consideration
- Full edit support for cloud saves may be added later
- Would require Steam cloud API write access or a copy-to-local workflow
