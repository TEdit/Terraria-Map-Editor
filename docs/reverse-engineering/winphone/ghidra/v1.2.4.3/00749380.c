
void FUN_00749380(void)

{
  int iVar1;
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
  
  uVar4 = FUN_0082a0b8();
  piVar3 = (int *)((ulonglong)uVar4 >> 0x20);
  iVar1 = (int)uVar4;
  (**(code **)(*piVar3 + 0x24))(piVar3,local_50,1,*(code **)(*piVar3 + 0x24),0xfffffffe,0xfffffffe);
  if (local_50[0] == '\x01') {
    if (extraout_r2 < 0x31) {
      FUN_007d2668(auStack_4c);
      FUN_006ea2e0(piVar3,auStack_4c);
      FUN_00771da0(auStack_bc,auStack_4c);
      FUN_007d24cc(iVar1 + 4,auStack_84,auStack_bc);
      FUN_007d2414(auStack_84);
      *(undefined1 *)(iVar1 + 0x34) = local_8c;
      *(undefined1 *)(iVar1 + 0x35) = local_8b;
      *(undefined1 *)(iVar1 + 0x36) = local_8a;
      thunk_FUN_007d2414(auStack_bc);
      (**(code **)(*piVar3 + 0x24))(piVar3,iVar1,2);
      (**(code **)(*piVar3 + 0x24))(piVar3,iVar1 + 2,2);
      FUN_007d2414(auStack_4c);
    }
    else {
      (**(code **)(*piVar3 + 0x24))(piVar3,iVar1,2);
      (**(code **)(*piVar3 + 0x24))(piVar3,iVar1 + 2,2);
      iVar2 = FUN_0077216c(auStack_84,piVar3,extraout_r2);
      FUN_007d24cc(iVar1 + 4,auStack_4c,iVar2);
      FUN_007d2414(auStack_4c);
      *(undefined1 *)(iVar1 + 0x34) = *(undefined1 *)(iVar2 + 0x30);
      *(undefined1 *)(iVar1 + 0x35) = *(undefined1 *)(iVar2 + 0x31);
      *(undefined1 *)(iVar1 + 0x36) = *(undefined1 *)(iVar2 + 0x32);
      thunk_FUN_007d2414(auStack_84);
    }
    FUN_00749100(iVar1);
  }
  FUN_0082a0d0();
  return;
}

