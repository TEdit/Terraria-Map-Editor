
void FUN_00749890(void)

{
  int *piVar1;
  undefined1 local_1c [4];
  undefined4 local_18 [2];
  
  piVar1 = (int *)FUN_007ffb80();
  local_1c[0] = DAT_010299a0;
  (**(code **)(*piVar1 + 0x18))(piVar1,local_1c,1);
  local_18[0] = DAT_010299a4;
  (**(code **)(*piVar1 + 0x18))(piVar1,local_18,4);
  local_18[0] = DAT_010299a8;
  (**(code **)(*piVar1 + 0x18))(piVar1,local_18,4);
  local_18[0] = DAT_010299b4;
  (**(code **)(*piVar1 + 0x18))(piVar1,local_18,4);
  FUN_007ffb98();
  return;
}

