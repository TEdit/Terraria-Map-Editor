
void FUN_00783400(int param_1,int param_2,int param_3)

{
  short sVar1;
  byte bVar2;
  bool bVar3;
  uint uVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  uint uVar8;
  bool bVar9;
  uint uVar10;
  int iVar11;
  int iVar12;
  int iVar13;
  uint uVar14;
  uint uVar15;
  uint in_fpscr;
  float fVar16;
  float fVar17;
  
  uVar4 = DAT_00fd67fc;
  uVar15 = DAT_00fd67f8;
  uVar10 = DAT_00fd67f4;
  fVar17 = DAT_00783498;
  uVar8 = DAT_00fd67f8;
  uVar14 = DAT_00fd6800;
  if ((DAT_010338d3 != '\0') && (DAT_00fe1f8b != '\0')) {
    DAT_00fd67f4 = DAT_00fd67f8;
    uVar10 = uVar10 ^ uVar10 << 0xb;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar14 = DAT_00fd6800 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar10;
    DAT_00fd67fc = DAT_00fd6800;
    fVar16 = (float)VectorSignedToFloat(uVar14 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    uVar8 = uVar4;
    uVar10 = uVar15;
    if ((int)(fVar16 * DAT_00783498 * 30.0) == 0) {
      DAT_00fd6800 = uVar14;
      FUN_007765c4(param_1,param_2);
      return;
    }
  }
  DAT_00fd67f4 = uVar8;
  DAT_00fd67f8 = DAT_00fd67fc;
  uVar15 = uVar14;
  if (DAT_010338d3 != '\0') {
    uVar10 = uVar10 ^ uVar10 << 0xb;
    uVar15 = uVar14 ^ (uVar10 ^ uVar14 >> 0xb) >> 8 ^ uVar10;
    fVar16 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67f4 = DAT_00fd67fc;
    DAT_00fd67f8 = uVar14;
    uVar10 = uVar8;
    if ((int)(fVar16 * DAT_00783498 * 10.0) == 0) {
      iVar5 = param_1 + -0x1e;
      bVar9 = true;
      if (iVar5 < param_1 + 0x1e) {
        iVar11 = DAT_00902934 * iVar5;
        iVar6 = param_2 + -0x1e;
        do {
          for (; iVar6 < param_2 + 0x1e; iVar6 = iVar6 + 2) {
            if ((((1 < iVar5) && (iVar5 < DAT_00902ea0 + -2)) && (1 < iVar6)) &&
               (iVar6 < DAT_00902ea4 + -2)) {
              iVar7 = (iVar11 + iVar6) * 0xe + DAT_00902928;
              if ((*(short *)(iVar7 + 6) == 0xec) && ((*(byte *)(iVar7 + 1) & 1) != 0)) {
                bVar3 = true;
              }
              else {
                bVar3 = false;
              }
              if (bVar3) {
                bVar9 = false;
                break;
              }
            }
          }
          if (!bVar9) break;
          iVar5 = iVar5 + 2;
          iVar11 = iVar11 + DAT_00902934 * 2;
          iVar6 = param_2 + -0x1e;
        } while (iVar5 < param_1 + 0x1e);
      }
      if (!bVar9) {
        DAT_00fd67f4 = uVar8;
        DAT_00fd67f8 = DAT_00fd67fc;
        DAT_00fd67fc = uVar14;
        DAT_00fd6800 = uVar15;
        return;
      }
      uVar8 = uVar8 ^ uVar8 << 0xb;
      DAT_00fd6800 = uVar15 ^ (uVar8 ^ uVar15 >> 0xb) >> 8 ^ uVar8;
      fVar17 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      DAT_00fd67fc = uVar15;
      if (((param_1 < 5) || (DAT_00902ea0 + -5 < param_1)) ||
         ((param_3 < 5 || (DAT_00902ea4 + -5 < param_3)))) {
LAB_0078374c:
        bVar9 = false;
      }
      else {
        iVar5 = param_1 + -1;
        if (iVar5 < param_1 + 1) {
          iVar6 = DAT_00902934 * iVar5;
          iVar11 = param_3 + -1;
          iVar7 = (iVar6 + param_3 + 1) * 0xe + DAT_00902928;
          do {
            if (iVar11 < param_3 + 1) {
              iVar12 = (iVar6 + iVar11) * 0xe + DAT_00902928;
              iVar13 = iVar11;
              do {
                sVar1 = *(short *)(iVar12 + 6);
                if ((sVar1 == 0xb9) && (*(short *)(iVar12 + 0xc) == 0)) {
                  bVar9 = true;
                }
                else {
                  bVar9 = false;
                }
                if ((((((*(byte *)(iVar12 + 1) & 1) != 0) && (sVar1 != 0x3d)) && (sVar1 != 0x3e)) &&
                    (((sVar1 != 0x45 && (sVar1 != 0x4a)) && ((sVar1 != 0xe9 && (!bVar9)))))) ||
                   (sVar1 == 5)) goto LAB_0078374c;
                iVar13 = iVar13 + 1;
                iVar12 = iVar12 + 0xe;
              } while (iVar13 < param_3 + 1);
            }
            if (*(short *)(iVar7 + 6) != 0x3c) goto LAB_0078374c;
            bVar2 = *(byte *)(iVar7 + 1);
            if ((((bVar2 & 1) == 0) || ((bVar2 & 2) != 0)) ||
               (((bVar2 & 0x18) != 0 || (((bVar2 & 4) != 0 || ((DAT_01026690 & 3) != 1)))))) {
              bVar9 = false;
            }
            else {
              bVar9 = true;
            }
            if (!bVar9) goto LAB_0078374c;
            iVar5 = iVar5 + 1;
            iVar6 = iVar6 + DAT_00902934;
            iVar7 = DAT_00902934 * 0xe + iVar7;
          } while (iVar5 < param_1 + 1);
        }
        sVar1 = (short)(int)(fVar17 * DAT_00783498 * 3.0) * 0x24;
        iVar5 = (DAT_00902934 * param_1 + param_3) * 0xe + DAT_00902928;
        *(undefined2 *)(iVar5 + 6) = 0xec;
        *(short *)(iVar5 + 10) = sVar1 + 0x12;
        *(byte *)(iVar5 + 1) = *(byte *)(iVar5 + 1) | 1;
        *(undefined2 *)(iVar5 + 0xc) = 0x12;
        *(short *)(iVar5 + -4) = sVar1 + 0x12;
        *(undefined2 *)(iVar5 + -8) = 0xec;
        *(byte *)(iVar5 + -0xd) = *(byte *)(iVar5 + -0xd) | 1;
        *(undefined2 *)(iVar5 + -2) = 0;
        *(undefined2 *)(iVar5 + -0x36b8) = 0xec;
        *(short *)(iVar5 + -0x36b4) = sVar1;
        *(byte *)(iVar5 + -0x36bd) = *(byte *)(iVar5 + -0x36bd) | 1;
        *(undefined2 *)(iVar5 + -0x36b2) = 0;
        *(undefined2 *)(iVar5 + -0x36aa) = 0xec;
        *(byte *)(iVar5 + -13999) = *(byte *)(iVar5 + -13999) | 1;
        bVar9 = true;
        *(short *)(iVar5 + -0x36a6) = sVar1;
        *(undefined2 *)(iVar5 + -0x36a4) = 0x12;
      }
      if (!bVar9) {
        return;
      }
      FUN_007729e8(param_1,param_3,0xffffffff);
      FUN_007729e8(param_1 + 1,param_3 + 1,0xffffffff);
      FUN_006496ec(param_1,param_3,4);
      return;
    }
  }
  uVar10 = uVar10 ^ uVar10 << 0xb;
  DAT_00fd6800 = uVar15 ^ (uVar10 ^ uVar15 >> 0xb) >> 8 ^ uVar10;
  fVar16 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  DAT_00fd67fc = uVar15;
  iVar5 = FUN_00754a00(param_1,param_3,0xe9,(int)(fVar16 * DAT_00783498 * 8.0),0);
  if (iVar5 != 0) {
    FUN_006496ec(param_1,param_3,4);
    return;
  }
  uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
  DAT_00fd67f4 = DAT_00fd67f8;
  DAT_00fd67f8 = DAT_00fd67fc;
  uVar10 = DAT_00fd6800 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar10;
  DAT_00fd67fc = DAT_00fd6800;
  fVar16 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  DAT_00fd6800 = uVar10;
  if ((((param_1 < 5) || (DAT_00902ea0 + -5 < param_1)) || (param_3 < 5)) ||
     (DAT_00902ea4 + -5 < param_3)) {
LAB_007839d6:
    bVar9 = false;
  }
  else {
    iVar5 = param_1 + -1;
    if (iVar5 < param_1 + 1) {
      iVar6 = iVar5 * DAT_00902934;
      iVar11 = param_3 + -1;
      iVar7 = (iVar6 + param_3 + 1) * 0xe + DAT_00902928;
      do {
        if (iVar11 < param_3 + 1) {
          iVar12 = (iVar6 + iVar11) * 0xe + DAT_00902928;
          iVar13 = iVar11;
          do {
            sVar1 = *(short *)(iVar12 + 6);
            if ((sVar1 == 0xb9) && (*(short *)(iVar12 + 0xc) == 0)) {
              bVar9 = true;
            }
            else {
              bVar9 = false;
            }
            if (((((*(byte *)(iVar12 + 1) & 1) != 0) && (sVar1 != 0x3d)) &&
                ((sVar1 != 0x3e && (((sVar1 != 0x45 && (sVar1 != 0x4a)) && (!bVar9)))))) ||
               (sVar1 == 5)) goto LAB_007839d6;
            iVar13 = iVar13 + 1;
            iVar12 = iVar12 + 0xe;
          } while (iVar13 < param_3 + 1);
        }
        if (*(short *)(iVar7 + 6) != 0x3c) goto LAB_007839d6;
        bVar2 = *(byte *)(iVar7 + 1);
        if ((((bVar2 & 1) == 0) || ((bVar2 & 2) != 0)) ||
           (((bVar2 & 0x18) != 0 || (((bVar2 & 4) != 0 || ((DAT_01026690 & 3) != 1)))))) {
          bVar9 = false;
        }
        else {
          bVar9 = true;
        }
        if (!bVar9) goto LAB_007839d6;
        iVar5 = iVar5 + 1;
        iVar6 = iVar6 + DAT_00902934;
        iVar7 = DAT_00902934 * 0xe + iVar7;
      } while (iVar5 < param_1 + 1);
    }
    sVar1 = (short)(int)(fVar16 * fVar17 * 12.0) * 0x24;
    iVar5 = (DAT_00902934 * param_1 + param_3) * 0xe + DAT_00902928;
    *(undefined2 *)(iVar5 + 6) = 0xe9;
    *(short *)(iVar5 + 10) = sVar1 + 0x12;
    *(byte *)(iVar5 + 1) = *(byte *)(iVar5 + 1) | 1;
    *(undefined2 *)(iVar5 + 0xc) = 0x36;
    *(short *)(iVar5 + -4) = sVar1 + 0x12;
    *(undefined2 *)(iVar5 + -8) = 0xe9;
    *(byte *)(iVar5 + -0xd) = *(byte *)(iVar5 + -0xd) | 1;
    *(undefined2 *)(iVar5 + -2) = 0x24;
    *(undefined2 *)(iVar5 + -0x36b8) = 0xe9;
    *(short *)(iVar5 + -0x36b4) = sVar1;
    *(byte *)(iVar5 + -0x36bd) = *(byte *)(iVar5 + -0x36bd) | 1;
    *(undefined2 *)(iVar5 + -0x36b2) = 0x24;
    *(undefined2 *)(iVar5 + -0x36aa) = 0xe9;
    *(byte *)(iVar5 + -13999) = *(byte *)(iVar5 + -13999) | 1;
    bVar9 = true;
    *(short *)(iVar5 + -0x36a6) = sVar1;
    *(undefined2 *)(iVar5 + -0x36a4) = 0x36;
  }
  if (bVar9) {
    FUN_006496ec(param_1,param_3,3);
  }
  return;
}

