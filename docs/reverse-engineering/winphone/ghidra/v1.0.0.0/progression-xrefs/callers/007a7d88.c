
void FUN_007a7d88(void)

{
  undefined1 *puVar1;
  uint in_fpscr;
  float fVar2;
  
  if (DAT_010339a0 == 0) {
    return;
  }
  if (DAT_010339a8 < 1) {
    if (DAT_010339a0 == 1) {
      DAT_00fe1f83 = 1;
      FUN_00736ecc(0x11);
      FUN_00647dc0(0);
    }
    else {
      if (DAT_010339a0 == 2) {
        puVar1 = &DAT_00fe1f84;
      }
      else {
        if (DAT_010339a0 != 3) goto LAB_007a7de4;
        puVar1 = &DAT_00fe1f87;
      }
      *puVar1 = 1;
    }
LAB_007a7de4:
    FUN_007a7a48();
    DAT_010339a0 = 0;
    DAT_010339ac = 7;
  }
  fVar2 = (float)VectorSignedToFloat((int)DAT_010338cc,(byte)(in_fpscr >> 0x16) & 3);
  if (DAT_010339a4 == fVar2) {
    return;
  }
  if (DAT_010339a4 <= fVar2) {
    if (fVar2 <= DAT_010339a4) goto LAB_007a7e66;
    DAT_010339a4 = DAT_010339a4 + 1.0;
    if (DAT_010339a4 < fVar2) goto LAB_007a7e2e;
  }
  else {
    DAT_010339a4 = DAT_010339a4 - 1.0;
    if (fVar2 < DAT_010339a4) {
LAB_007a7e2e:
      DAT_010339b0 = DAT_010339b0 + -1;
      goto LAB_007a7e66;
    }
  }
  DAT_010339a4 = fVar2;
  FUN_007a7a48();
LAB_007a7e66:
  if (DAT_010339b0 < 1) {
    DAT_010339b0 = 0xe10;
    FUN_007a7a48();
  }
  return;
}

