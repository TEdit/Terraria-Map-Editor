
void FUN_00762228(void)

{
  undefined4 uVar1;
  int iVar2;
  int iVar3;
  undefined *puVar4;
  undefined2 *puVar5;
  uint local_1b0 [4];
  undefined4 local_1a0;
  uint local_19c;
  undefined2 *local_198;
  undefined2 *local_194;
  undefined4 local_190;
  undefined4 uStack_18c;
  undefined1 auStack_188 [48];
  undefined1 auStack_158 [48];
  undefined4 local_128;
  undefined2 auStack_124 [134];
  
  local_190 = 0xfffffffe;
  uStack_18c = 0xfffffffe;
  local_19c = 0xf;
  local_1a0 = 0;
  local_1b0[0] = local_1b0[0] & 0xffffff00;
  FUN_0041bc2c(local_1b0,"GEN[47]",7);
  uVar1 = FUN_005f82b0(auStack_158,local_1b0);
  iVar2 = FUN_0058ef48();
  FUN_007aa80c(iVar2 + 0x2b10,auStack_188,uVar1);
  FUN_007aa754(auStack_188);
  FUN_007aa754(auStack_158);
  if (0xf < local_19c) {
    FUN_00431740(local_1b0[0]);
  }
  local_19c = 0xf;
  local_1a0 = 0;
  local_1b0[0] = local_1b0[0] & 0xffffff00;
  FUN_007350e8(0x3f800000,&DAT_01030850);
  uVar1 = DAT_007624d0;
  DAT_0102a764 = 0;
  DAT_0102a760 = 0;
  DAT_0102a75c = 0;
  DAT_0102a758 = 0;
  DAT_0102a754 = 0;
  DAT_0102a750 = 0;
  DAT_0102a768 = 0;
  DAT_0102a76c = 0;
  DAT_0102a70e = 0;
  DAT_0102a70f = 0;
  DAT_0102a778 = 0;
  DAT_0102a77c = 0;
  DAT_0102a76f = 0;
  DAT_0102a780 = 0;
  DAT_0102a781 = 0;
  DAT_00902c30 = 0xffff;
  DAT_00902c34 = 0xffff;
  DAT_00902c38 = 0xffff;
  DAT_01033974 = 0;
  DAT_01033970 = 0;
  DAT_010338d3 = 0;
  DAT_01033968 = 0;
  DAT_01033964 = 0;
  DAT_010339ac = 0;
  DAT_010339a0 = 0;
  DAT_010339a8 = 0;
  DAT_010339b0 = 0;
  DAT_010339a4 = DAT_007624d0;
  DAT_00fe1c70 = 0;
  DAT_00fe1c98 = 0;
  iVar2 = 0x3f;
  puVar5 = auStack_124;
  do {
    iVar2 = iVar2 + -1;
    puVar5[1] = 0;
    *puVar5 = 0;
    puVar5 = puVar5 + 2;
  } while (-1 < iVar2);
  local_128 = 0;
  memcpy(&DAT_01030648,&local_128,0x104);
  iVar2 = 0x3f;
  puVar5 = auStack_124;
  do {
    iVar2 = iVar2 + -1;
    puVar5[1] = 0;
    *puVar5 = 0;
    puVar5 = puVar5 + 2;
  } while (-1 < iVar2);
  local_128 = 0;
  memcpy(&DAT_0103074c,&local_128,0x104);
  DAT_0102a784 = DAT_00902ea0;
  DAT_0102a788 = DAT_00902ea4;
  FUN_00667810();
  DAT_010299b4 = uVar1;
  DAT_010299a0 = 0;
  memset(DAT_00902928,0,DAT_0090292c * 0xe);
  FUN_0060cf6c(&DAT_01073158);
  iVar2 = 0x13d8;
  do {
    FUN_006117c0(iVar2 + DAT_00fd68c0);
    iVar2 = iVar2 + -0x28;
  } while (-1 < iVar2);
  puVar4 = &DAT_010731a0;
  iVar2 = 200;
  do {
    FUN_00612a6c(puVar4);
    puVar4 = puVar4 + 0xa0;
    iVar2 = iVar2 + -1;
  } while (iVar2 != 0);
  puVar5 = &DAT_0107af40;
  iVar2 = 0x200;
  do {
    FUN_006ef2e0(puVar5);
    puVar5 = puVar5 + 0x74;
    iVar2 = iVar2 + -1;
    iVar3 = DAT_007624cc;
  } while (iVar2 != 0);
  do {
    iVar2 = iVar3 + -0x1908;
    *(undefined1 *)(DAT_00fd643c + iVar3 + 0x1904) = 1;
    iVar3 = iVar2;
  } while (-1 < iVar2);
  FUN_0075942c();
  puVar5 = &DAT_01060308;
  iVar2 = 0x4000;
  do {
    if (puVar5 != (undefined2 *)0x0) {
      local_198 = puVar5;
      local_194 = puVar5;
      FUN_00640a40(puVar5);
    }
    puVar5 = puVar5 + 2;
    iVar2 = iVar2 + -1;
  } while (iVar2 != 0);
  return;
}

