
void FUN_0073527c(void)

{
  int iVar1;
  int *piVar2;
  undefined8 uVar3;
  undefined4 local_14;
  
  uVar3 = FUN_007ffb80();
  piVar2 = (int *)((ulonglong)uVar3 >> 0x20);
  iVar1 = (int)uVar3;
  local_14 = *(undefined4 *)(iVar1 + 4);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,4);
  local_14._0_1_ = *(undefined1 *)(iVar1 + 8);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
  local_14._0_1_ = *(undefined1 *)(iVar1 + 0xc);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
  local_14 = CONCAT31(local_14._1_3_,*(undefined1 *)(iVar1 + 10));
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
  local_14 = CONCAT22(local_14._2_2_,*(undefined2 *)(iVar1 + 0xe));
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,2);
  local_14._0_1_ = *(undefined1 *)(iVar1 + 9);
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
  local_14 = CONCAT31(local_14._1_3_,(char)*(undefined2 *)(iVar1 + 0x12) + 'o');
  (**(code **)(*piVar2 + 0x18))(piVar2,&local_14,1);
  FUN_007ffb98();
  return;
}

