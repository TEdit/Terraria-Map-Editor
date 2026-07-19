
void FUN_007a7a48(void)

{
  undefined4 uVar1;
  undefined4 in_r3;
  uint in_fpscr;
  float fVar2;
  
  if (DAT_010339a8 < 1) {
    if (DAT_010339a0 == 2) {
      uVar1 = 4;
    }
    else if (DAT_010339a0 == 3) {
      uVar1 = 0x2a;
    }
    else {
      uVar1 = 0;
    }
  }
  else {
    fVar2 = (float)VectorSignedToFloat((int)DAT_010338cc,(byte)(in_fpscr >> 0x16) & 3);
    if (fVar2 <= DAT_010339a4) {
      if (DAT_010339a4 <= fVar2) {
        if (DAT_010339a0 == 2) {
          uVar1 = 7;
        }
        else if (DAT_010339a0 == 3) {
          uVar1 = 0x2d;
        }
        else {
          uVar1 = 3;
        }
      }
      else if (DAT_010339a0 == 2) {
        uVar1 = 6;
      }
      else if (DAT_010339a0 == 3) {
        uVar1 = 0x2c;
      }
      else {
        uVar1 = 2;
      }
    }
    else if (DAT_010339a0 == 2) {
      uVar1 = 5;
    }
    else if (DAT_010339a0 == 3) {
      uVar1 = 0x2b;
    }
    else {
      uVar1 = 1;
    }
  }
  FUN_00646f20(uVar1,0xaf,0x4b,0xff,0xffffffff,in_r3);
  return;
}

