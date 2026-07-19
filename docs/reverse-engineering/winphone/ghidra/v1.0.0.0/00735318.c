
void FUN_00735318(void)

{
  undefined4 *puVar1;
  int *piVar2;
  int extraout_r2;
  undefined8 uVar3;
  byte local_1c [8];
  
  uVar3 = FUN_007ffb80();
  piVar2 = (int *)((ulonglong)uVar3 >> 0x20);
  puVar1 = (undefined4 *)uVar3;
  *puVar1 = 0x3f800000;
  (**(code **)(*piVar2 + 0x1c))(piVar2,puVar1 + 1,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,puVar1 + 2,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,puVar1 + 3,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,(int)puVar1 + 10,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,(int)puVar1 + 0xe,2);
  if (0x39 < extraout_r2) {
    (**(code **)(*piVar2 + 0x1c))(piVar2,(int)puVar1 + 9,1);
    (**(code **)(*piVar2 + 0x1c))(piVar2,local_1c,1);
    *(ushort *)((int)puVar1 + 0x12) = local_1c[0] + 0x1291;
  }
  FUN_007ffb98();
  return;
}

