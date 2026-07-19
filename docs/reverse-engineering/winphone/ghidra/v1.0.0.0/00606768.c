
void FUN_00606768(void)

{
  int iVar1;
  int *piVar2;
  undefined8 uVar3;
  undefined2 local_14;
  
  uVar3 = FUN_007ffb80();
  piVar2 = (int *)((ulonglong)uVar3 >> 0x20);
  iVar1 = (int)uVar3;
  if (*(char *)(iVar1 + 0x1904) != '\0') {
    local_14 = (ushort)local_14._1_1_ << 8;
    (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
    FUN_007ffb98();
    return;
  }
  local_14 = CONCAT11(local_14._1_1_,1);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
  local_14 = *(undefined2 *)(iVar1 + 0x1900);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,2);
  local_14 = *(undefined2 *)(iVar1 + 0x1902);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,2);
  FUN_00605b9c(iVar1,piVar2);
  FUN_007ffb98();
  return;
}

