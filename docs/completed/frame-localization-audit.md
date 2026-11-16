# Terraria Frame 本地化审计与修复报告

- 日期：2026-11-17
- 分支：`fix/terraria-localization-display`（PR #2535）
- 语言：zh-Hans
- 范围：`tiles.json` 全部 9,546 个 Frame，对照官方 en-US / zh-Hans、`LocalizationOverrides.zh-Hans.json`、`supplementalNames.*.json`、`items.json` / `tiles.json` 稳定 Key，并运行实际 `ResolveFrameDisplayName()`。

## 结论

| 指标 | 修复前 | 修复后 |
| --- | ---: | ---: |
| 显示非英文源文 | 2,158 | 2,247 |
| 回退完整英文源文 | 7,388 | 7,299 |
| 精确 Frame 覆盖命中 | 812 | 812 |

共改变 144 个 Frame：

- 17 个 Boss Trophy 不再重复追加 `Trophy`，且 `Ancient Cultist Trophy` 正确映射到官方 `AncientCultistTrophy`（`Lunatic Cultist Trophy`）。
- 110 个冗余类别组合改为直接显示完整物品名（墓石、金属锭、字母雕像、部分杂物/矿石堆等）。
- 17 个历史拼写/旧家具名通过明确别名映射到稳定 Item Key。
- 没有出现“中文变英文”、新增原文回退或旧行为退化。

## 已确认并修复

### Boss Trophy（Tile 240，29 个）

修复前 12/29 正常，17/29 回退原文。原因：旧变体的 `Variety` 不含 `Trophy`，新变体已含，旧逻辑无条件追加 `Trophy`。

现在先匹配完整物品名，再尝试追加后缀：

| 原始 Frame | 修复后 |
| --- | --- |
| `Boss Trophy: Mourning Wood Trophy` | 哀木纪念章 |
| `Boss Trophy: Pumpking Trophy` | 南瓜王纪念章 |
| `Boss Trophy: King Slime Trophy` | 史莱姆王纪念章 |
| `Boss Trophy: Moon Lord Trophy` | 月亮领主纪念章 |
| `Boss Trophy: Ogre Trophy` | 食人魔纪念章 |
| `Boss Trophy: Ancient Cultist Trophy` | 拜月教邪教徒纪念章 |
| `Boss Trophy: Queen Slime Trophy` | 史莱姆皇后纪念章 |
| `Boss Trophy: Deerclops Trophy` | 独眼巨鹿纪念章 |

### Fishing Trophy（4 个）

当前版本修复前已全部正常（`金鱼纪念章`、`兔兔鱼纪念章`、`剑鱼纪念章`、`鲨牙纪念章`）。本次保留“完整物品名优先”规则，并由测试锁定不退化。

### Tombstones（Tile 85，11 个）

修复前为 `墓石: 墓石`、`墓石: 墓碑` 等冗余组合；现在直接复用完整物品译名：

`墓石`、`墓石碑`、`十字墓石碑`、`碑石`、`墓碑`、`方尖碑`、`金十字墓石碑`、`金墓石`、`金墓石碑`、`金墓碑`、`金碑石`。

### MetalBars（Tile 239，23 个）

修复前为 `金属锭: 铜锭` 等；现在直接显示 `铜锭`、`锡锭`、`铁锭`……`夜明锭`。

### Banners（Tile 91）

10 个历史命名原本无法匹配当前物品名，现通过明确别名修复：

| Frame Variety | 官方 Item Key | 显示 |
| --- | --- | --- |
| `Zombie Eskimo Banner` | `ZombieEskimoBanner` | 冰冻僵尸旗 |
| `Skeleton Mage Banner` | `SkeletonMageBanner` | 暗黑法师旗 |
| `Nypmh Banner` | `NypmhBanner` | 宁芙旗 |
| `Blue Cultist Caster Banner` | `BlueCultistCasterBanner` | 拜月教信徒旗 |
| `Martian Brainscrambler Banner` | `MartianBrainscramblerBanner` | 火星扰脑怪旗 |
| `Goblin Summoner Banner` | `GoblinSummonerBanner` | 哥布林术士旗 |
| `Grolem Banner` | `GraniteGolemBanner` | 花岗岩魔像旗 |
| `Ravager Scorpion Banner` | `RavagerScorpionBanner` | 沙贼旗 |
| `Small Star Cell Banner` | `StardustSmallCellBanner` | 星尘小细胞旗 |
| `Hemoglobin Shark Banner` | `GoblinSharkBanner` | 血浆哥布林鲨鱼旗 |

其余 306 个 Banner 原本已正常。

### Paintings

- `Pillagin Me Pixels` → `PillaginMePixels`（`"Pillagin' Me Pixels"`）：`海贼像素画`。
- `Strange Growth A-D` 使用 `StrangeGrowth` 组合规则：`奇异植株 A/B/C/D`。
- Painting3X3 由 84/101 提升到 101/101；Painting2X3 由 28/32 提升到 32/32；Painting6X4 由 74/75 提升到 75/75。

### 其他经确认的别名

| 历史 Frame 名 | 官方 Item Key |
| --- | --- |
| `Pirahna Statue` | `PiranhaStatue` |
| `Web Coverd Chest` | `WebCoveredChest` |
| `Trapped Web Coverd Chest` | `Fake_WebCoveredChest` |
| `Gold Crate` | `GoldenCrate` |
| `Meteor Toilet` | `ToiletMeteor` |
| `Diablost Lamp` | `DiablostLamp` |
| `Oil Rag Sconse` | `OilRagSconse` |
| `Wooden Work Bench` | `WorkBench` |
| `Boreal Work Bench` | `BorealWoodWorkBench` |
| `Logic Gate (NXOR)` | `LogicGate_NXOR` |
| `Dynasty Platform` | `DynastyPlatform` |
| `Spooky Platform` | `SpookyPlatform` |
| `Barstool` | `BarStool` |
| `Metal Bathtub` | `Bathtub` |
| `Large Dynasty Candelabra` | `DynastyCandelabra` |
| `Lihzahrd Locked Door` | `LihzahrdDoor` |
| `Music Box (Ocean)` | `MusicBoxOcean` |
| `Music Box (Space)` | `MusicBoxSpace` |
| `Wooden Bed` / `Bench` / `Sofa` / `Bookcase` / `Dresser` / `Piano` / `Chest` | `Bed` / `Bench` / `Sofa` / `Bookcase` / `Dresser` / `Piano` / `Chest` |
| `Blinkroot Planter` 等 8 个种植盆 | `BlinkrootPlanterBox` 等 |
| `A Statue` … `Z Statue` / `0 Statue` … `9 Statue` | `AlphabetStatueA` … `AlphabetStatue9` |

## 剩余原文回退分类

7,299 个 Frame 仍保留英文原文，按实际资源归因：

| 原因 | 数量 | 示例 |
| --- | ---: | --- |
| 基础名有译名，但 Variety/风格词缺少 zh-Hans supplemental 词条 | 6,363 | `Torch: On [Bottom]`、`Armor Statue: Left`、`Wood Platform: Flat`、`Music Box (Overworld Day): Off`、`Wooden Door: A` |
| 基础名和风格名都没有官方译名 | 924 | `Forest Tree: Trunk Plain A`、`Forest Pot: A1`、`Cattail: Forest Cattail` |
| 资源本身标注不使用的占位 Frame | 12 | `Crystal Chest (unused - DO NOT USE)`、`Trapped Barrel (unused - DO NOT USE)` |

这不是名称匹配 Bug。`supplementalNames.en-US.json` 含有 1,396 个身份映射，但 `supplementalNames.zh-Hans.json` 目前只有 8 个锚点/墙体词条；要显示这些 Frame，需要 Crowdin 补充方向、开关、连接形态、字母/数字样式等词汇。按任务要求，本次没有批量写入未经验证的人工翻译。

特别注意：166 个普通 Statue 全部回退原文，是因为 `Left`/`Right` 缺失，而不是基础物品名缺失；`Pirahna Statue` 别名已就位，词汇补齐后即可正常显示。

## 实现要点

- 新增 `_localizedItemNamesByKey`，历史别名只映射到稳定 Item Key，不写入官方生成 JSON。
- 别名表为显式白名单，不使用模糊匹配。
- 只有“类别名 + 完整物品名”时才省略类别前缀；`Bone A1`、`Skull Right` 等复合风格仍保留类别。
- 基础名无法翻译但 Variety 有完整名称时（画作、Boss/Trophy、墙面挂饰），直接使用完整名称并保留 Anchor。
- Boss Trophy 始终先尝试完整 Variety，再尝试 `Variety + " Trophy"`。
- 所有规则只依赖当前语言的官方 Item/Tile/Supplemental 数据，不硬编码简体中文。

## 验证

- `TEdit.Terraria.Tests`：639 通过，19 跳过，0 失败。
- `TEdit.Tests`：678 通过，6 失败；失败均为缺少 `_reference_sources/winphone/.../Tutorial.world` 的既有环境问题，与本次修改无关。
- `dotnet build src/TEdit/TEdit.csproj -c Release`：0 警告，0 错误。
- 新增 1 个定向测试，覆盖 Boss Trophy、Fishing Trophy、墓石、金属锭、Banner 别名、画作、字母雕像、中英文搜索及重复后缀/冗余类别回归检查。
