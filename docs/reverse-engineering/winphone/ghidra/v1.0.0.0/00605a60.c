
void FUN_00605a60(void)

{
  undefined4 uVar1;
  int *piVar2;
  uint uVar3;
  int extraout_r2;
  uint uVar4;
  int iVar5;
  uint uVar6;
  int iVar7;
  int iVar8;
  undefined8 uVar9;
  byte local_34 [2];
  ushort local_32;
  undefined1 local_30;
  undefined1 local_2f [3];
  byte local_2c [6];
  short local_26;
  
  uVar9 = FUN_007ffb80();
  piVar2 = (int *)((ulonglong)uVar9 >> 0x20);
  iVar7 = (int)uVar9;
  iVar8 = 0x28;
  iVar5 = iVar7;
  do {
    FUN_00612a6c(iVar5);
    iVar5 = iVar5 + 0xa0;
    iVar8 = iVar8 + -1;
  } while (iVar8 != 0);
  local_2c[4] = 0;
  local_2c[3] = 0;
  local_2c[2] = 0;
  local_2c[1] = 0;
  local_2c[0] = 0;
  if (extraout_r2 < 0x3a) {
    uVar6 = 0;
    do {
      uVar4 = uVar6 & 7;
      uVar3 = uVar6 >> 3;
      uVar6 = uVar6 + 1;
      local_2c[uVar3] = local_2c[uVar3] | (byte)(1 << uVar4);
    } while ((int)uVar6 < 0x14);
  }
  else {
    (**(code **)(*piVar2 + 0x1c))(piVar2,local_2f,1);
    (**(code **)(*piVar2 + 0x1c))(piVar2,local_2c,local_2f[0]);
  }
  iVar5 = 0x28;
  if (extraout_r2 < 0x3a) {
    iVar5 = 0x14;
  }
  uVar6 = 0;
  if (iVar5 != 0) {
    do {
      if ((1 << (uVar6 & 7) & (uint)local_2c[uVar6 >> 3]) != 0) {
        local_32 = 0;
        if (extraout_r2 < 0x37) {
          local_34[0] = 0;
          (**(code **)(*piVar2 + 0x1c))(piVar2,local_34,1);
          local_32 = (ushort)local_34[0];
        }
        else {
          (**(code **)(*piVar2 + 0x1c))(piVar2,&local_32,2);
        }
        if (0 < (short)local_32) {
          (**(code **)(*piVar2 + 0x1c))(piVar2,&local_26,2);
          (**(code **)(*piVar2 + 0x1c))(piVar2,&local_30,1);
          uVar1 = FUN_00613700((int)local_26,extraout_r2);
          iVar8 = FUN_006129f8(extraout_r2,uVar1);
          if (iVar8 < 0) {
            FUN_006317fc(iVar7,uVar1,(int)(short)local_32);
            FUN_00613df4(iVar7,local_30);
          }
        }
      }
      uVar6 = uVar6 + 1;
      iVar7 = iVar7 + 0xa0;
    } while ((int)uVar6 < iVar5);
  }
  FUN_007ffb98();
  return;
}

