
void FUN_00605b9c(void)

{
  int *piVar1;
  uint uVar2;
  int iVar3;
  int iVar4;
  undefined8 uVar5;
  undefined2 local_2c [4];
  byte local_24 [8];
  
  uVar5 = FUN_007ffb80();
  piVar1 = (int *)((ulonglong)uVar5 >> 0x20);
  iVar4 = (int)uVar5;
  uVar2 = 0;
  local_24[4] = 0;
  local_24[3] = 0;
  local_24[2] = 0;
  local_24[1] = 0;
  local_24[0] = 0;
  iVar3 = iVar4;
  do {
    if ((0 < *(short *)(iVar3 + 0x42)) && (*(int *)(iVar3 + 4) != 0)) {
      local_24[uVar2 >> 3] = local_24[uVar2 >> 3] | (byte)(1 << (uVar2 & 7));
    }
    uVar2 = uVar2 + 1;
    iVar3 = iVar3 + 0xa0;
  } while ((int)uVar2 < 0x28);
  local_2c[0] = CONCAT11(local_2c[0]._1_1_,5);
  (**(code **)(*piVar1 + 0x18))(piVar1,local_2c,1);
  (**(code **)(*piVar1 + 0x18))(piVar1,local_24,5);
  uVar2 = 0;
  do {
    if ((1 << (uVar2 & 7) & (uint)local_24[uVar2 >> 3]) != 0) {
      local_2c[0] = *(undefined2 *)(iVar4 + 0x42);
      (**(code **)(*piVar1 + 0x18))(piVar1,local_2c,2);
      local_2c[0] = *(undefined2 *)(iVar4 + 0x80);
      (**(code **)(*piVar1 + 0x18))(piVar1,local_2c,2);
      local_2c[0] = CONCAT11(local_2c[0]._1_1_,*(undefined1 *)(iVar4 + 0x2c));
      (**(code **)(*piVar1 + 0x18))(piVar1,local_2c,1);
    }
    uVar2 = uVar2 + 1;
    iVar4 = iVar4 + 0xa0;
  } while ((int)uVar2 < 0x28);
  FUN_007ffb98();
  return;
}

