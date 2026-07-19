
void FUN_0078d7b4(void)

{
  int *piVar1;
  undefined4 uVar2;
  int iVar3;
  undefined4 local_b4 [2];
  uint local_ac [4];
  undefined4 local_9c;
  uint local_98;
  undefined4 local_94;
  undefined4 uStack_90;
  undefined1 auStack_8c [48];
  int local_5c;
  int local_58;
  undefined1 auStack_54 [4];
  undefined4 local_50;
  undefined1 auStack_4c [52];
  
  piVar1 = (int *)FUN_0082a0b8();
  local_94 = 0xfffffffe;
  uStack_90 = 0xfffffffe;
  (**(code **)(*piVar1 + 0x24))(piVar1,&local_5c,4);
  if (0x46 < local_5c) goto LAB_0078d904;
  local_50 = 0;
  (**(code **)(*piVar1 + 0x24))(piVar1,&local_50,4);
  FUN_007d2668(auStack_4c);
  if (local_5c < 0x32) {
    uVar2 = FUN_004ac644(piVar1[0xf],local_ac);
    FUN_007d2d64(auStack_4c,uVar2);
    if (0xf < local_98) {
      FUN_00447c80(local_ac[0]);
    }
    local_98 = 0xf;
    local_9c = 0;
    local_ac[0] = local_ac[0] & 0xffffff00;
  }
  else {
    FUN_006ea2e0(piVar1,auStack_4c);
  }
  FUN_007d24cc(&DAT_00a9b4d8,auStack_8c,auStack_4c);
  FUN_007d2414(auStack_8c);
  if (local_5c < 0x35) {
LAB_0078d886:
    if (0x35 < local_5c) goto LAB_0078d88a;
LAB_0078d8ee:
    local_b4[0] = 0;
    FUN_00645dd0(&DAT_00ac34a4,local_b4);
  }
  else {
    (**(code **)(*piVar1 + 0x24))(piVar1,&DAT_00a592f6,1);
    if (local_5c < 0x36) {
      (**(code **)(*piVar1 + 0x24))(piVar1,&DAT_00a5935c,4);
      goto LAB_0078d886;
    }
LAB_0078d88a:
    std::_Container_base0::_Orphan_all((_Container_base0 *)&DAT_00ac34a4);
    DAT_00ac34a8 = DAT_00ac34a4;
    (**(code **)(*piVar1 + 0x24))(piVar1,&local_58,4);
    iVar3 = 0;
    if (0 < local_58) {
      do {
        (**(code **)(*piVar1 + 0x24))(piVar1,auStack_54,4);
        FUN_006d69f0(&DAT_00ac34a4,auStack_54);
        iVar3 = iVar3 + 1;
      } while (iVar3 < local_58);
    }
    if (DAT_00ac34a4 == DAT_00ac34a8) goto LAB_0078d8ee;
  }
  FUN_007d2414(auStack_4c);
LAB_0078d904:
  FUN_0082a0d0(local_5c);
  return;
}

