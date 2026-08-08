
void FUN_0078d0d8(void)

{
  int iVar1;
  int iVar2;
  int *piVar3;
  undefined4 uVar4;
  uint uVar5;
  undefined2 *puVar6;
  longlong lVar7;
  byte local_44 [2];
  short local_42;
  char local_40;
  char local_3f [3];
  undefined1 auStack_3c [4];
  uint local_38;
  undefined4 local_34;
  undefined4 local_30 [4];
  
  lVar7 = FUN_0082a0b8();
  uVar4 = (undefined4)((ulonglong)lVar7 >> 0x20);
  piVar3 = (int *)lVar7;
  if (0x41ffffffff < lVar7) {
    iVar1 = (**(code **)(*piVar3 + 0x30))();
    iVar2 = (**(code **)(*piVar3 + 0x2c))(piVar3);
    piVar3 = (int *)FUN_007783c8(piVar3,iVar1 - iVar2);
    if (piVar3 == (int *)0x0) goto LAB_0078d7a2;
  }
  (**(code **)(*piVar3 + 0x24))(piVar3,local_30,4);
  local_34 = 0;
  (**(code **)(*piVar3 + 0x24))(piVar3,&local_34,4);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00934f00,4);
  (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
  DAT_00934f04 = (int)local_42;
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00934f0c,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00934f08,2);
  FUN_007874c4();
  FUN_0078bfa8();
  DAT_00a59300 = local_30[0];
  DAT_00a592fc = local_34;
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a59254,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a59250,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
  DAT_00a7b4d4 = (int)local_42;
  DAT_00a9b538 = DAT_00a7b4d4 << 4;
  (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
  DAT_00a592ec = (int)local_42;
  DAT_00a592e8 = DAT_00a592ec << 4;
  if (lVar7 < 0x4300000000) {
    DAT_00a592e4 = DAT_00934f0c + -0xbe;
  }
  else {
    (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
    DAT_00a592e4 = (int)local_42;
  }
  DAT_00a5925c = DAT_00a592e4 << 4;
  FUN_00762bc4(&DAT_00a56160,piVar3,uVar4);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a592f4,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a592f0,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a13e8e,1);
  FUN_007754bc(piVar3,uVar4);
  FUN_00652af4(piVar3,uVar4);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a13e84,1);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a13e85,1);
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13e7c = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a13e80,4);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00934c38,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00934c3c,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00934c40,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a59257,1);
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a56170,2);
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a5934c = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
  DAT_00a59348 = (int)local_42;
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a59340 = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a59344,4);
  if (lVar7 < 0x4000000000) {
    FUN_005fb174();
  }
  else {
    FUN_005fb2c0(piVar3);
  }
  if (0x39ffffffff < lVar7) {
    iVar1 = 0;
    do {
      (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a13e50 + iVar1,1);
      iVar1 = iVar1 + 1;
    } while (iVar1 < 4);
    puVar6 = &DAT_00a13e54;
    iVar1 = 3;
    do {
      (**(code **)(*piVar3 + 0x24))(piVar3,puVar6,2);
      puVar6 = puVar6 + 1;
      iVar1 = iVar1 + -1;
    } while (iVar1 != 0);
    iVar1 = 0;
    do {
      (**(code **)(*piVar3 + 0x24))(piVar3,&DAT_00a13e44 + iVar1,1);
      iVar1 = iVar1 + 1;
    } while (iVar1 < 4);
    puVar6 = &DAT_00a13e48;
    iVar1 = 3;
    do {
      (**(code **)(*piVar3 + 0x24))(piVar3,puVar6,2);
      puVar6 = puVar6 + 1;
      iVar1 = iVar1 + -1;
    } while (iVar1 != 0);
  }
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  uVar5 = (uint)local_44[0];
  DAT_00a13e40 = uVar5 & 3;
  DAT_00a13e38 = (uVar5 & 0xf) >> 2;
  DAT_00a13e3c = (uVar5 & 0x3f) >> 4;
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13ee8 = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13eec = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13ef0 = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13ef4 = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13ef8 = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13efc = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13f00 = (uint)local_44[0];
  (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
  DAT_00a13f04 = (uint)local_44[0];
  if (lVar7 < 0x3e00000000) {
    DAT_00934c50 = 7;
    DAT_00934c54 = 6;
    DAT_00934c58 = 9;
    DAT_00934c5c = 8;
  }
  else {
    (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
    DAT_00934c50 = (int)local_42;
    (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
    DAT_00934c54 = (int)local_42;
    (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
    DAT_00934c58 = (int)local_42;
    (**(code **)(*piVar3 + 0x24))(piVar3,&local_42,2);
    DAT_00934c5c = (int)local_42;
  }
  (**(code **)(*piVar3 + 0x24))(piVar3,auStack_3c,4);
  FUN_0075bb00(piVar3,uVar4);
  (**(code **)(*piVar3 + 0x24))(piVar3,auStack_3c,4);
  FUN_005ec460(piVar3,uVar4);
  (**(code **)(*piVar3 + 0x24))(piVar3,auStack_3c,4);
  iVar1 = 0;
  do {
    FUN_00749380(iVar1 + DAT_00934724,piVar3,uVar4);
    iVar1 = iVar1 + 0x38;
  } while (iVar1 < 56000);
  (**(code **)(*piVar3 + 0x24))(piVar3,auStack_3c,4);
  (**(code **)(*piVar3 + 0x24))(piVar3,local_3f,1);
  local_40 = local_3f[0];
  iVar1 = FUN_007cf070();
  uVar4 = DAT_0078d7b0;
  if (iVar1 == 0) {
    if (local_40 == '\x01') {
      iVar1 = 0;
      do {
        local_38 = 0;
        if (lVar7 < 0x3f00000000) {
          (**(code **)(*piVar3 + 0x24))(piVar3,local_44,1);
          local_38 = (uint)local_44[0];
        }
        else {
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_38,4);
        }
        iVar2 = FUN_007cf070();
        if (iVar2 == 0) {
          FUN_006af0fc(uVar4,iVar1 + DAT_009656cc,local_38);
          (**(code **)(*piVar3 + 0x24))(piVar3,DAT_009656cc + iVar1 + 0x1b0,4);
          (**(code **)(*piVar3 + 0x24))(piVar3,DAT_009656cc + iVar1 + 0x1b4,4);
          *(int *)(iVar1 + DAT_009656cc + 0x1c0) = (int)*(float *)(iVar1 + DAT_009656cc + 0x1b0);
          *(int *)(iVar1 + DAT_009656cc + 0x1c4) = (int)*(float *)(iVar1 + DAT_009656cc + 0x1b4);
          (**(code **)(*piVar3 + 0x24))(piVar3,iVar1 + DAT_009656cc + 0x194,1);
          (**(code **)(*piVar3 + 0x24))(piVar3,DAT_009656cc + iVar1 + 0x272,2);
          (**(code **)(*piVar3 + 0x24))(piVar3,DAT_009656cc + iVar1 + 0x274,2);
        }
        iVar1 = iVar1 + 0x2f4;
        (**(code **)(*piVar3 + 0x24))(piVar3,&local_40,1);
      } while (local_40 == '\x01');
    }
    (**(code **)(*piVar3 + 0x24))(piVar3,auStack_3c,4);
    FUN_006ea2e0(piVar3,&DAT_00965a08);
    FUN_006ea2e0(piVar3,&DAT_00965a38);
    FUN_006ea2e0(piVar3,&DAT_00965a68);
    FUN_006ea2e0(piVar3,&DAT_00965a98);
    FUN_006ea2e0(piVar3,&DAT_00965af8);
    FUN_006ea2e0(piVar3,&DAT_009660f8);
    FUN_006ea2e0(piVar3,&DAT_00965df8);
    FUN_006ea2e0(piVar3,&DAT_00966ae8);
    FUN_006ea2e0(piVar3,&DAT_00966b18);
    FUN_006ea2e0(piVar3,&DAT_00966e18);
    FUN_006ea2e0(piVar3,&DAT_009674d8);
    FUN_006ea2e0(piVar3,&DAT_00967838);
    FUN_006ea2e0(piVar3,&DAT_00967da8);
    FUN_006ea2e0(piVar3,&DAT_00967e08);
    FUN_006ea2e0(piVar3,&DAT_00967dd8);
    FUN_006ea2e0(piVar3,&DAT_00968168);
    FUN_006ea2e0(piVar3,&DAT_00968198);
    FUN_006ea2e0(piVar3,&DAT_009681c8);
    FUN_006ea2e0(piVar3,&DAT_00969c08);
  }
  iVar1 = FUN_00564548();
  *(undefined4 *)(iVar1 + 0x2e98) = 0x3f800000;
LAB_0078d7a2:
  FUN_0082a0d0();
  return;
}

