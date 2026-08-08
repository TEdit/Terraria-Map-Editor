
void FUN_007a8f74(int param_1)

{
  uint uVar1;
  int iVar2;
  uint in_fpscr;
  float fVar3;
  
  if ((DAT_010339a0 == 0) && (DAT_010339ac == 0)) {
    uVar1 = (uint)(*(char *)(DAT_0107034c + 0x5cc5) != '\0');
    if (*(char *)(DAT_01070350 + 0x5cc5) != '\0') {
      uVar1 = uVar1 + 1;
    }
    if (*(char *)(DAT_01070354 + 0x5cc5) != '\0') {
      uVar1 = uVar1 + 1;
    }
    if (*(char *)(DAT_01070358 + 0x5cc5) != '\0') {
      uVar1 = uVar1 + 1;
    }
    if (uVar1 != 0) {
      DAT_010339a8 = uVar1 * 0x28 + 0x1e;
      DAT_010339b0 = 0;
      uVar1 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar1 = DAT_00fd6800 ^ (uVar1 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar1;
      DAT_00fd67fc = DAT_00fd6800;
      fVar3 = (float)VectorSignedToFloat(uVar1 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      DAT_00fd6800 = uVar1;
      DAT_010339a0 = param_1;
      DAT_010339a4 = DAT_007a906c;
      if ((int)(fVar3 * DAT_007a9070 * 2.0) != 0) {
        DAT_010339a4 = VectorSignedToFloat((int)DAT_00902ea0,(byte)(in_fpscr >> 0x16) & 3);
      }
    }
  }
  iVar2 = (&DAT_0107034c)[DAT_0103399c];
  if (*(char *)(iVar2 + 0x5d23) == '\x1c') {
    FUN_007a8384(0x1f,(int)*(float *)(iVar2 + 0x110),(int)*(float *)(iVar2 + 0x114),5);
    return;
  }
  return;
}

