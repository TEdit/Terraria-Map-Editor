
void FUN_0074904c(void)

{
  short *psVar1;
  int iVar2;
  int *piVar3;
  undefined8 uVar4;
  undefined2 local_14;
  
  uVar4 = FUN_0082a0b8();
  piVar3 = (int *)((ulonglong)uVar4 >> 0x20);
  psVar1 = (short *)uVar4;
  if ((-1 < *psVar1) && (iVar2 = thunk_FUN_007d2340(psVar1 + 2), iVar2 == 0)) {
    local_14 = CONCAT11(local_14._1_1_,1);
    (**(code **)(*piVar3 + 0x20))(piVar3,&local_14,1);
    local_14 = *psVar1;
    (**(code **)(*piVar3 + 0x20))(piVar3,&local_14,2);
    local_14 = psVar1[1];
    (**(code **)(*piVar3 + 0x20))(piVar3,&local_14,2);
    FUN_00772040(psVar1 + 2,piVar3);
    FUN_0082a0d0();
    return;
  }
  local_14 = (ushort)local_14._1_1_ << 8;
  (**(code **)(*piVar3 + 0x20))(piVar3,&local_14,1);
  FUN_0082a0d0();
  return;
}

