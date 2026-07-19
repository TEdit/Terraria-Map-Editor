
void FUN_006452c4(void)

{
  int iVar1;
  short sVar2;
  int iVar3;
  int *piVar4;
  undefined1 uVar5;
  byte bVar6;
  int iVar7;
  ushort uVar8;
  int iVar9;
  undefined4 local_2c [2];
  
  iVar3 = FUN_007ffb80();
  EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e5c);
  DAT_00fe1e74 = 1;
  (**(code **)(DAT_00fe1d38 + 0x24))(&DAT_00fe1d38);
  local_2c[0] = CONCAT31(local_2c[0]._1_3_,(char)iVar3);
  (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
  iVar1 = DAT_00fe1ee4;
  if (iVar3 < 0x3b) {
    if (iVar3 != 0x3a) {
      if (iVar3 == 7) {
        local_2c[0] = DAT_010703dc;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,4);
        bVar6 = DAT_010703e4 << 5;
        if (DAT_010703e0 != '\0') {
          bVar6 = bVar6 | 1;
        }
        if (DAT_010703e1 != '\0') {
          bVar6 = bVar6 | 2;
        }
        if (DAT_010703e2 != '\0') {
          bVar6 = bVar6 | 4;
        }
        if (DAT_010703e3 != '\0') {
          bVar6 = bVar6 | 8;
        }
        local_2c[0] = CONCAT31(local_2c[0]._1_3_,bVar6);
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_2_ = DAT_00902ea0;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        local_2c[0]._0_2_ = DAT_00902ea4;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        local_2c[0]._0_2_ = DAT_010338cc;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        local_2c[0]._0_2_ = DAT_010338c8;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        local_2c[0]._0_2_ = (undefined2)DAT_00902ebc;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        local_2c[0] = CONCAT22(local_2c[0]._2_2_,(short)DAT_01033960);
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        local_2c[0] = DAT_01033974;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,4);
        local_2c[0] = DAT_01033970;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,4);
        FUN_00644b44(&DAT_0102a710,&DAT_00fe1d38);
        FUN_00644acc(&DAT_0102a704,&DAT_00fe1d38);
        local_2c[0]._0_1_ = (byte)DAT_0102a700 | (byte)((DAT_0102a6f8 | DAT_0102a6fc << 2) << 2);
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a71c;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a720;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a724;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a728;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a72c;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a730;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a734;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0]._0_1_ = (byte)DAT_0102a738;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0] = CONCAT31(local_2c[0]._1_3_,(char)DAT_00901160);
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        local_2c[0] = DAT_01029998;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,4);
        if (DAT_010299a0 == '\0') {
          DAT_010299a8 = DAT_00645804;
        }
        DAT_010299ac = DAT_010299a8;
        local_2c[0] = DAT_010299a8;
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,4);
        uVar8 = (ushort)(DAT_0102a76f != '\0');
        if (DAT_00fe1f73 != '\0') {
          uVar8 = uVar8 | 2;
        }
        if (DAT_00fe1f7a != '\0') {
          uVar8 = uVar8 | 4;
        }
        if (DAT_00fe1f7b != '\0') {
          uVar8 = uVar8 | 8;
        }
        if (DAT_010338d3 != '\0') {
          uVar8 = uVar8 | 0x10;
        }
        if (DAT_00fe1f85 != '\0') {
          uVar8 = uVar8 | 0x20;
        }
        if (DAT_00fe1f8d != '\0') {
          uVar8 = uVar8 | 0x40;
        }
        if (DAT_00fe1f88 != '\0') {
          uVar8 = uVar8 | 0x80;
        }
        if (DAT_00fe1f89 != '\0') {
          uVar8 = uVar8 | 0x100;
        }
        if (DAT_00fe1f8a != '\0') {
          uVar8 = uVar8 | 0x200;
        }
        if (DAT_0102a787 != '\0') {
          uVar8 = uVar8 | 0x400;
        }
        if (1.0 <= DAT_010299b4) {
          uVar8 = uVar8 | 0x800;
        }
        sVar2 = FUN_00734160(&DAT_010703d8);
        local_2c[0] = CONCAT22(local_2c[0]._2_2_,uVar8 | sVar2 << 0xc);
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,2);
        (**(code **)(DAT_00fe1d38 + 0x38))(&DAT_00fe1d38,&DAT_01070308);
      }
      else if ((iVar3 == 0xb) && (DAT_00fe1ec4 == 2)) {
        iVar3 = 0;
        uVar5 = 0;
        iVar7 = *(int *)(DAT_00fe1ee4 + 0x24);
        if (0 < iVar7) {
          piVar4 = *(int **)(DAT_00fe1ee4 + 0x1c);
          do {
            iVar3 = iVar3 + *(int *)(*piVar4 + 0x14);
            uVar5 = (undefined1)iVar3;
            iVar7 = iVar7 + -1;
            piVar4 = piVar4 + 1;
          } while (iVar7 != 0);
        }
        local_2c[0] = CONCAT31(local_2c[0]._1_3_,uVar5);
        (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
        iVar3 = 0;
        if (0 < *(int *)(iVar1 + 0x24)) {
          do {
            iVar9 = *(int *)(*(int *)(iVar1 + 0x1c) + iVar3 * 4);
            local_2c[0] = CONCAT31(local_2c[0]._1_3_,(char)*(undefined4 *)(iVar9 + 0x14));
            (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
            iVar7 = 0;
            if (0 < *(int *)(iVar9 + 0x14)) {
              do {
                local_2c[0] = CONCAT31(local_2c[0]._1_3_,
                                       *(undefined1 *)
                                        (*(int *)(*(int *)(iVar9 + 0xc) + iVar7 * 4) + 0x5cc9));
                (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
                iVar7 = iVar7 + 1;
              } while (iVar7 < *(int *)(iVar9 + 0x14));
            }
            iVar3 = iVar3 + 1;
          } while (iVar3 < *(int *)(iVar1 + 0x24));
        }
      }
      goto LAB_006457e6;
    }
    local_2c[0]._0_1_ = DAT_0102a70f;
    (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
    local_2c[0] = CONCAT31(local_2c[0]._1_3_,DAT_0102a70e);
  }
  else {
    if (iVar3 != 0x53) goto LAB_006457e6;
    local_2c[0] = DAT_010703dc;
    (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,4);
    bVar6 = DAT_010703e4 << 2;
    if (DAT_010703e0 != '\0') {
      bVar6 = bVar6 | 1;
    }
    if (DAT_010703e2 != '\0') {
      bVar6 = bVar6 | 2;
    }
    local_2c[0] = CONCAT31(local_2c[0]._1_3_,bVar6);
  }
  (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_2c,1);
LAB_006457e6:
  LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e5c);
  DAT_00fe1e74 = 0;
  FUN_007ffb98();
  return;
}

