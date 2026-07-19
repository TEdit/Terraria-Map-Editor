
void FUN_005e9ef8(void)

{
  int iVar1;
  int *piVar2;
  undefined8 uVar3;
  undefined2 local_14;
  
  uVar3 = FUN_0082a0b8();
  piVar2 = (int *)((ulonglong)uVar3 >> 0x20);
  iVar1 = (int)uVar3;
  if (*(char *)(iVar1 + 0x1ae4) != '\0') {
    local_14 = (ushort)local_14._1_1_ << 8;
    (**(code **)(*piVar2 + 0x20))(piVar2,&local_14,1);
    FUN_0082a0d0();
    return;
  }
  local_14 = CONCAT11(local_14._1_1_,1);
  (**(code **)(*piVar2 + 0x20))(piVar2,&local_14,1);
  local_14 = *(undefined2 *)(iVar1 + 0x1ae0);
  (**(code **)(*piVar2 + 0x20))(piVar2,&local_14,2);
  local_14 = *(undefined2 *)(iVar1 + 0x1ae2);
  (**(code **)(*piVar2 + 0x20))(piVar2,&local_14,2);
  FUN_005e8de8(iVar1,piVar2);
  FUN_0082a0d0();
  return;
}

