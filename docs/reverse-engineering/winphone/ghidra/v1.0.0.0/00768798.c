
void FUN_00768798(void)

{
  byte bVar1;
  int *piVar2;
  int iVar3;
  undefined4 uVar4;
  undefined4 uVar5;
  undefined4 uVar6;
  int iVar7;
  uint uVar8;
  undefined2 *puVar9;
  longlong lVar10;
  uint local_94 [4];
  undefined4 local_84;
  uint local_80;
  undefined4 local_7c;
  undefined4 uStack_78;
  undefined1 auStack_74 [48];
  byte local_44 [2];
  short local_42;
  char local_40;
  char local_3f [3];
  undefined1 auStack_3c [4];
  undefined4 local_38;
  undefined4 local_34 [4];
  
  lVar10 = FUN_007ffb80();
  uVar6 = (undefined4)((ulonglong)lVar10 >> 0x20);
  piVar2 = (int *)lVar10;
  local_7c = 0xfffffffe;
  uStack_78 = 0xfffffffe;
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_34,4);
  local_38 = 0;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_38,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902e98,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_00902e9c = (int)local_42;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902ea4,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902ea0,2);
  iVar3 = (int)DAT_00902ea4;
  DAT_00902e9c = iVar3 << 4;
  DAT_00902e98 = (int)DAT_00902ea0 << 4;
  iVar7 = (int)((ulonglong)((longlong)(int)DAT_00902ea0 * (longlong)DAT_00768ec8) >> 0x20);
  DAT_010502fc = (iVar7 >> 3) - (iVar7 >> 0x1f);
  iVar3 = (int)((ulonglong)((longlong)iVar3 * (longlong)DAT_00768ec4) >> 0x20) + iVar3;
  DAT_01050300 = (iVar3 >> 3) - (iVar3 >> 0x1f);
  FUN_00762228();
  DAT_01033974 = local_34[0];
  DAT_01033970 = local_38;
  local_80 = 0xf;
  local_84 = 0;
  local_94[0] = local_94[0] & 0xffffff00;
  FUN_0041bc2c(local_94,"GEN[51]",7);
  uVar4 = FUN_005f82b0(auStack_74,local_94);
  uVar5 = FUN_0058ef48();
  FUN_007369b0(uVar5,4,uVar4);
  FUN_007aa754(auStack_74);
  if (0xf < local_80) {
    FUN_00431740(local_94[0]);
  }
  local_80 = 0xf;
  local_84 = 0;
  local_94[0] = local_94[0] & 0xffffff00;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010338cc,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010338c8,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_00902ebc = (int)local_42;
  DAT_01050304 = DAT_00902ebc << 4;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_01033960 = (int)local_42;
  DAT_0103395c = DAT_01033960 << 4;
  iVar3 = (int)((ulonglong)
                ((longlong)((DAT_00902ea4 - DAT_00902ebc) + -0xe6) * (longlong)DAT_00768ec0) >> 0x20
               );
  DAT_01033958 = DAT_00902ebc + (iVar3 - (iVar3 >> 0x1f)) * 6 + -5;
  DAT_010338d4 = DAT_01033958 * 0x10;
  FUN_00735318(&DAT_01030850,piVar2,uVar6);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_01033968,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_01033964,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a787,1);
  FUN_00749d20(piVar2,uVar6);
  FUN_00659a38(piVar2,uVar6);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a76f,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a780,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a778 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a77c,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902c30,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902c34,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902c38,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010338d3,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_01030860,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_010339ac = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_010339a8 = (int)local_42;
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_010339a0 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010339a4,4);
  if (0x39ffffffff < lVar10) {
    iVar3 = 0;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a710 + iVar3,1);
      iVar3 = iVar3 + 1;
    } while (iVar3 < 4);
    puVar9 = &DAT_0102a714;
    iVar3 = 3;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,puVar9,2);
      puVar9 = puVar9 + 1;
      iVar3 = iVar3 + -1;
    } while (iVar3 != 0);
    iVar3 = 0;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a704 + iVar3,1);
      iVar3 = iVar3 + 1;
    } while (iVar3 < 4);
    puVar9 = &DAT_0102a708;
    iVar3 = 3;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,puVar9,2);
      puVar9 = puVar9 + 1;
      iVar3 = iVar3 + -1;
    } while (iVar3 != 0);
  }
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  uVar8 = (uint)local_44[0];
  DAT_0102a700 = uVar8 & 3;
  DAT_0102a6f8 = (uVar8 & 0xf) >> 2;
  DAT_0102a6fc = (uVar8 & 0x3f) >> 4;
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a71c = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a720 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a724 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a728 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a72c = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a730 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a734 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
  DAT_0102a738 = (uint)local_44[0];
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  FUN_00733858(piVar2,uVar6);
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  FUN_006082a4(piVar2,uVar6);
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  iVar3 = 0;
  do {
    FUN_00726440(iVar3 + DAT_009027f0,piVar2,uVar6);
    iVar3 = iVar3 + 0x38;
  } while (iVar3 < 56000);
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_3f,1);
  local_40 = local_3f[0];
  iVar3 = FUN_007a7d34();
  if (iVar3 == 0) {
    if (local_40 == '\x01') {
      iVar3 = 0;
      do {
        (**(code **)(*piVar2 + 0x1c))(piVar2,local_44,1);
        bVar1 = local_44[0];
        iVar7 = FUN_007a7d34();
        if (iVar7 == 0) {
          FUN_0066c0b8(0xbf800000,iVar3 + DAT_00fe1fa8,bVar1);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 0x138,4);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 0x13c,4);
          *(int *)(iVar3 + DAT_00fe1fa8 + 0x148) = (int)*(float *)(iVar3 + DAT_00fe1fa8 + 0x138);
          *(int *)(iVar3 + DAT_00fe1fa8 + 0x14c) = (int)*(float *)(iVar3 + DAT_00fe1fa8 + 0x13c);
          (**(code **)(*piVar2 + 0x1c))(piVar2,iVar3 + DAT_00fe1fa8 + 0x119,1);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 0x1f2,2);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 500,2);
        }
        iVar3 = iVar3 + 0x274;
        (**(code **)(*piVar2 + 0x1c))(piVar2,&local_40,1);
      } while (local_40 == '\x01');
    }
    (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
    FUN_006d6c24(piVar2,&DAT_00fe22e0);
    FUN_006d6c24(piVar2,&DAT_00fe2310);
    FUN_006d6c24(piVar2,&DAT_00fe2340);
    FUN_006d6c24(piVar2,&DAT_00fe2370);
    FUN_006d6c24(piVar2,&DAT_00fe23d0);
    FUN_006d6c24(piVar2,&DAT_00fe29d0);
    FUN_006d6c24(piVar2,&DAT_00fe26d0);
    FUN_006d6c24(piVar2,&DAT_00fe33c0);
    FUN_006d6c24(piVar2,&DAT_00fe33f0);
    FUN_006d6c24(piVar2,&DAT_00fe36f0);
    FUN_006d6c24(piVar2,&DAT_00fe3db0);
    FUN_006d6c24(piVar2,&DAT_00fe4110);
    FUN_006d6c24(piVar2,&DAT_00fe4680);
    FUN_006d6c24(piVar2,&DAT_00fe46e0);
    FUN_006d6c24(piVar2,&DAT_00fe46b0);
    FUN_006d6c24(piVar2,&DAT_00fe4a40);
    FUN_006d6c24(piVar2,&DAT_00fe4a70);
    FUN_006d6c24(piVar2,&DAT_00fe4aa0);
  }
  FUN_007ffb98();
  return;
}

