
undefined8 FUN_00667810(void)

{
  int iVar1;
  int iVar2;
  int iVar3;
  short sVar4;
  
  FUN_0065942c();
  iVar1 = DAT_006678a4;
  sVar4 = 0;
  DAT_00fe1f71 = 0;
  DAT_00fe1f70 = 0;
  DAT_00fe1f6c = 0;
  DAT_00fe1f7c = 0;
  DAT_00fe1f73 = 0;
  DAT_00fe1f7a = 0;
  DAT_00fe1f7b = 0;
  DAT_00fe1f7e = 0;
  DAT_00fe1f7f = 0;
  DAT_00fe1f82 = 0;
  DAT_00fe1f83 = 0;
  DAT_00fe1f85 = 0;
  DAT_00fe1f84 = 0;
  DAT_00fe1f87 = 0;
  DAT_00fe1f88 = 0;
  DAT_00fe1f89 = 0;
  DAT_00fe1f8a = 0;
  DAT_00fe1f8b = 0;
  DAT_00fe1f8c = 0;
  DAT_00fe1f8d = 0;
  iVar3 = 0;
  do {
    iVar2 = iVar3 + DAT_00fe1fa8;
    FUN_0065a2d8(iVar2);
    if (iVar2 != 0) {
      FUN_0065a358(iVar2);
    }
    *(short *)(DAT_00fe1fa8 + iVar3 + 0x1e6) = sVar4;
    iVar3 = iVar3 + 0x274;
    sVar4 = sVar4 + 1;
  } while (iVar3 < iVar1);
  return CONCAT44(iVar2,iVar2);
}

