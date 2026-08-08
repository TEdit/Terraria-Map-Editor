
void FUN_00726440(void)

{
  short *psVar1;
  int iVar2;
  int *piVar3;
  int extraout_r2;
  undefined8 uVar4;
  undefined1 auStack_bc [48];
  undefined1 local_8c;
  undefined1 local_8b;
  undefined1 local_8a;
  undefined1 auStack_84 [52];
  char local_50 [4];
  undefined1 auStack_4c [52];
  
  uVar4 = FUN_007ffb80();
  piVar3 = (int *)((ulonglong)uVar4 >> 0x20);
  psVar1 = (short *)uVar4;
  (**(code **)(*piVar3 + 0x1c))(piVar3,local_50,1,*(code **)(*piVar3 + 0x1c),0xfffffffe,0xfffffffe);
  if (local_50[0] == '\x01') {
    if (extraout_r2 < 0x31) {
      FUN_007aa9a8(auStack_4c);
      FUN_006d6c24(piVar3,auStack_4c);
      FUN_0074624c(auStack_bc,auStack_4c);
      FUN_007aa80c(psVar1 + 2,auStack_84,auStack_bc);
      FUN_007aa754(auStack_84);
      *(undefined1 *)(psVar1 + 0x1a) = local_8c;
      *(undefined1 *)((int)psVar1 + 0x35) = local_8b;
      *(undefined1 *)(psVar1 + 0x1b) = local_8a;
      thunk_FUN_007aa754(auStack_bc);
      (**(code **)(*piVar3 + 0x1c))(piVar3,psVar1,2);
      (**(code **)(*piVar3 + 0x1c))(piVar3,psVar1 + 1,2);
      FUN_007aa754(auStack_4c);
    }
    else {
      (**(code **)(*piVar3 + 0x1c))(piVar3,psVar1,2);
      (**(code **)(*piVar3 + 0x1c))(piVar3,psVar1 + 1,2);
      iVar2 = FUN_0074660c(auStack_84,piVar3,extraout_r2);
      FUN_007aa80c(psVar1 + 2,auStack_4c,iVar2);
      FUN_007aa754(auStack_4c);
      *(undefined1 *)(psVar1 + 0x1a) = *(undefined1 *)(iVar2 + 0x30);
      *(undefined1 *)((int)psVar1 + 0x35) = *(undefined1 *)(iVar2 + 0x31);
      *(undefined1 *)(psVar1 + 0x1b) = *(undefined1 *)(iVar2 + 0x32);
      thunk_FUN_007aa754(auStack_84);
    }
    FUN_007261a0(psVar1);
  }
  else if (-1 < *psVar1) {
    thunk_FUN_007aa680(psVar1 + 2);
  }
  FUN_007ffb98();
  return;
}

