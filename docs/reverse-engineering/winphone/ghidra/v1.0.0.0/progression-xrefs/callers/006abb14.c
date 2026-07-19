
int FUN_006abb14(int *param_1)

{
  uint uVar1;
  uint uVar2;
  undefined1 uVar3;
  int iVar4;
  int iVar5;
  uint uVar6;
  uint uVar7;
  int iVar8;
  int iVar9;
  uint uVar10;
  uint uVar11;
  int iVar12;
  int iVar13;
  uint in_fpscr;
  float fVar14;
  float fVar15;
  undefined4 uVar16;
  
  uVar2 = DAT_00fd6800;
  uVar1 = DAT_00fd67fc;
  uVar7 = DAT_00fd67f8;
  uVar6 = DAT_00fd67f4;
  fVar14 = DAT_006abfe4;
  iVar8 = param_1[1];
  iVar13 = *param_1;
  iVar12 = param_1[3];
  iVar4 = param_1[5];
  uVar10 = param_1[2] - (int)DAT_010338cc >> 0x1f;
  iVar9 = (param_1[2] - (int)DAT_010338cc ^ uVar10) - uVar10;
  iVar5 = (int)((ulonglong)((longlong)(int)DAT_00902ea0 * (longlong)DAT_006abc24) >> 0x20);
  if (iVar5 - (iVar5 >> 0x1f) <= iVar9) {
    if (iVar9 <= DAT_00902ea0) {
      return -1;
    }
    uVar10 = DAT_00fd6800;
    uVar11 = DAT_00fd67fc;
    if (iVar4 == 2) {
      DAT_00fd67f4 = DAT_00fd67f8;
      uVar6 = uVar6 ^ uVar6 << 0xb;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar10 = DAT_00fd6800 ^ (uVar6 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar6;
      DAT_00fd67fc = DAT_00fd6800;
      fVar15 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      uVar11 = uVar2;
      uVar6 = uVar7;
      uVar7 = uVar1;
      if (((int)(fVar15 * DAT_006abfe4 * DAT_006abfe0) == 0) &&
         (DAT_00fd6800 = uVar10, iVar4 = FUN_00656370(0x32), iVar4 == 0)) {
        iVar4 = FUN_00685550(iVar13,iVar8,0x32,0);
        return iVar4;
      }
    }
    uVar6 = uVar6 ^ uVar6 << 0xb;
    DAT_00fd6800 = uVar6 ^ (uVar6 ^ uVar10 >> 0xb) >> 8 ^ uVar10;
    fVar15 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67f4 = uVar7;
    DAT_00fd67f8 = uVar11;
    DAT_00fd67fc = uVar10;
    if ((int)(fVar15 * fVar14 * 15.0) != 0) {
      if (DAT_00fe1f83 != '\0') {
        return -1;
      }
      if (DAT_0102a76f == '\0') {
        return -1;
      }
      iVar4 = FUN_00608330(&DAT_00fd67f4,7);
      if (iVar4 != 0) {
        return -1;
      }
    }
    iVar5 = 0;
    iVar4 = DAT_00fe1fa8;
    while (*(char *)(iVar4 + 0x100) != '\0') {
      iVar5 = iVar5 + 1;
      iVar4 = iVar4 + 0x274;
      if (0xc3 < iVar5) {
        return 0xc4;
      }
    }
    if (0xc3 < iVar5) {
      return iVar5;
    }
    FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,0x49);
    iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
    iVar13 = iVar13 - (uint)(*(ushort *)(iVar4 + 0x168) >> 1);
    goto LAB_006abf56;
  }
  DAT_00fd67f4 = DAT_00fd67f8;
  uVar6 = uVar6 ^ uVar6 << 0xb;
  DAT_00fd67f8 = DAT_00fd67fc;
  uVar6 = DAT_00fd6800 ^ (uVar6 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar6;
  DAT_00fd67fc = DAT_00fd6800;
  fVar14 = (float)VectorSignedToFloat(uVar6 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  if ((int)(fVar14 * DAT_006abc20 * 15.0) == 0) {
    if ((iVar4 != 2) && (iVar4 != 0x6d)) {
      if (iVar4 == 0x93) goto LAB_006abcc2;
      if (iVar4 != 0xa1) goto LAB_006abd88;
    }
    if ((iVar4 == 0x93) || (iVar4 == 0xa1)) {
LAB_006abcc2:
      uVar7 = uVar7 ^ uVar7 << 0xb;
      DAT_00fd67f4 = uVar1;
      uVar7 = uVar6 ^ (uVar7 ^ uVar6 >> 0xb) >> 8 ^ uVar7;
      DAT_00fd67f8 = DAT_00fd6800;
      fVar14 = (float)VectorSignedToFloat(uVar7 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      if ((int)(fVar14 * DAT_006abc20 * 2.0) == 0) {
        iVar5 = 0;
        iVar4 = DAT_00fe1fa8;
        do {
          if (*(char *)(iVar4 + 0x100) == '\0') {
            if (0xc3 < iVar5) {
              DAT_00fd67fc = uVar6;
              DAT_00fd6800 = uVar7;
              return iVar5;
            }
            DAT_00fd67fc = uVar6;
            DAT_00fd6800 = uVar7;
            FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,0x94);
            iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
            iVar13 = iVar13 - (uint)(*(ushort *)(iVar4 + 0x168) >> 1);
            goto LAB_006abf56;
          }
          iVar5 = iVar5 + 1;
          iVar4 = iVar4 + 0x274;
        } while (iVar5 < 0xc4);
      }
      else {
        iVar5 = 0;
        iVar4 = DAT_00fe1fa8;
        do {
          if (*(char *)(iVar4 + 0x100) == '\0') {
            if (0xc3 < iVar5) {
              DAT_00fd67fc = uVar6;
              DAT_00fd6800 = uVar7;
              return iVar5;
            }
            DAT_00fd67fc = uVar6;
            DAT_00fd6800 = uVar7;
            FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,0x95);
            iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
            iVar13 = iVar13 - (uint)(*(ushort *)(iVar4 + 0x168) >> 1);
            goto LAB_006abf56;
          }
          iVar5 = iVar5 + 1;
          iVar4 = iVar4 + 0x274;
        } while (iVar5 < 0xc4);
      }
      DAT_00fd67fc = uVar6;
      DAT_00fd6800 = uVar7;
      return 0xc4;
    }
    DAT_00fd6800 = uVar6;
    iVar4 = FUN_007354a8();
    if ((iVar4 == 0) || (iVar4 = FUN_00608330(&DAT_00fd67f4,3), iVar4 == 0)) {
      iVar4 = FUN_00608330(&DAT_00fd67f4,3);
      if ((iVar4 != 0) || (DAT_00902ebc < iVar12)) {
        FUN_006921b0(iVar13,iVar8);
        return -1;
      }
      iVar5 = 0;
      iVar4 = DAT_00fe1fa8;
      while (*(char *)(iVar4 + 0x100) != '\0') {
        iVar5 = iVar5 + 1;
        iVar4 = iVar4 + 0x274;
        if (0xc3 < iVar5) {
          return 0xc4;
        }
      }
      if (0xc3 < iVar5) {
        return iVar5;
      }
      FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,299);
      iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
      iVar13 = iVar13 - (uint)(*(ushort *)(iVar4 + 0x168) >> 1);
    }
    else {
      iVar5 = 0;
      iVar4 = DAT_00fe1fa8;
      while (*(char *)(iVar4 + 0x100) != '\0') {
        iVar5 = iVar5 + 1;
        iVar4 = iVar4 + 0x274;
        if (0xc3 < iVar5) {
          return 0xc4;
        }
      }
      if (0xc3 < iVar5) {
        return iVar5;
      }
      FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,0x12f);
      iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
      iVar13 = iVar13 - (uint)(*(ushort *)(iVar4 + 0x168) >> 1);
    }
  }
  else {
LAB_006abd88:
    uVar7 = uVar7 ^ uVar7 << 0xb;
    DAT_00fd67f4 = uVar1;
    uVar7 = uVar6 ^ (uVar7 ^ uVar6 >> 0xb) >> 8 ^ uVar7;
    DAT_00fd67f8 = DAT_00fd6800;
    fVar14 = (float)VectorSignedToFloat(uVar7 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar14 * DAT_006abc20 * 15.0) != 0) {
      DAT_00fd67fc = uVar6;
      DAT_00fd6800 = uVar7;
      return -1;
    }
    if (((iVar4 != 2) && (iVar4 != 0x6d)) && (iVar4 != 0x93)) {
      DAT_00fd67fc = uVar6;
      DAT_00fd6800 = uVar7;
      return -1;
    }
    iVar5 = 0;
    iVar4 = DAT_00fe1fa8;
    while (*(char *)(iVar4 + 0x100) != '\0') {
      iVar5 = iVar5 + 1;
      iVar4 = iVar4 + 0x274;
      if (0xc3 < iVar5) {
        DAT_00fd67fc = uVar6;
        DAT_00fd6800 = uVar7;
        return 0xc4;
      }
    }
    if (0xc3 < iVar5) {
      DAT_00fd67fc = uVar6;
      DAT_00fd6800 = uVar7;
      return iVar5;
    }
    DAT_00fd67fc = uVar6;
    DAT_00fd6800 = uVar7;
    FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,0x4a);
    iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
    iVar13 = iVar13 - (uint)(*(ushort *)(iVar4 + 0x168) >> 1);
  }
LAB_006abf56:
  *(int *)(iVar4 + 0x148) = iVar13;
  uVar16 = VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
  *(undefined4 *)(iVar4 + 0x138) = uVar16;
  iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
  iVar8 = iVar8 - (uint)*(ushort *)(iVar4 + 0x16a);
  *(int *)(iVar4 + 0x14c) = iVar8;
  uVar16 = VectorSignedToFloat(iVar8,(byte)(in_fpscr >> 0x16) & 3);
  *(undefined4 *)(iVar4 + 0x13c) = uVar16;
  *(undefined1 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
  *(undefined4 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
  iVar4 = iVar5 * 0x274 + DAT_00fe1fa8;
  uVar3 = FUN_0060a19c(iVar4 + 0x138,*(undefined2 *)(iVar4 + 0x168),*(undefined2 *)(iVar4 + 0x16a));
  *(undefined1 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 100) = uVar3;
  FUN_00649e08(iVar5,1);
  return iVar5;
}

