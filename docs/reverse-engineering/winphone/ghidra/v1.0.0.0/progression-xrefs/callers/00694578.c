
int FUN_00694578(int *param_1)

{
  bool bVar1;
  uint uVar2;
  uint uVar3;
  char cVar4;
  undefined1 uVar5;
  int iVar6;
  int iVar7;
  uint uVar8;
  int iVar9;
  int iVar10;
  int iVar11;
  int iVar12;
  uint in_fpscr;
  undefined4 uVar13;
  float fVar14;
  undefined1 auStack_58 [52];
  
  iVar12 = param_1[2];
  iVar7 = param_1[3];
  iVar10 = *param_1;
  iVar11 = param_1[1];
  iVar9 = -1;
  bVar1 = false;
  if ((DAT_00fe1f8d != '\0') && (DAT_010338d3 != '\0')) {
    bVar1 = true;
  }
  if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xe5) != '\0') {
    iVar9 = 0;
    if (((*(char *)((DAT_00902934 * iVar12 + iVar7) * 0xe + DAT_00902928 + 8) == '^') ||
        (*(char *)((DAT_00902934 * iVar12 + iVar7) * 0xe + DAT_00902928 + 8) == '`')) ||
       (*(char *)((DAT_00902934 * iVar12 + iVar7) * 0xe + DAT_00902928 + 8) == 'b')) {
      iVar9 = 1;
    }
    else if (((*(char *)((DAT_00902934 * iVar12 + iVar7) * 0xe + DAT_00902928 + 8) == '_') ||
             (*(char *)((DAT_00902934 * iVar12 + iVar7) * 0xe + DAT_00902928 + 8) == 'a')) ||
            (*(char *)((DAT_00902934 * iVar12 + iVar7) * 0xe + DAT_00902928 + 8) == 'c')) {
      iVar9 = 2;
    }
    if (DAT_00fe1f7b == '\0') {
      iVar9 = 0xc4;
      iVar12 = 0;
      iVar7 = DAT_00fe1fa8;
      do {
        if (*(char *)(iVar7 + 0x100) == '\0') {
          if (0xc3 < iVar12) {
            return iVar12;
          }
          FUN_00684770(iVar12 * 0x274 + DAT_00fe1fa8,0x44);
          iVar9 = iVar12 * 0x274 + DAT_00fe1fa8;
          iVar10 = iVar10 - (uint)(*(ushort *)(iVar9 + 0x168) >> 1);
          *(int *)(iVar9 + 0x148) = iVar10;
          uVar13 = VectorSignedToFloat(iVar10,(byte)(in_fpscr >> 0x16) & 3);
          *(undefined4 *)(iVar9 + 0x138) = uVar13;
          iVar9 = iVar12 * 0x274 + DAT_00fe1fa8;
          iVar11 = iVar11 - (uint)*(ushort *)(iVar9 + 0x16a);
          *(int *)(iVar9 + 0x14c) = iVar11;
          uVar13 = VectorSignedToFloat(iVar11,(byte)(in_fpscr >> 0x16) & 3);
          *(undefined4 *)(iVar9 + 0x13c) = uVar13;
          *(undefined1 *)(iVar12 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
          *(undefined4 *)(iVar12 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
          iVar9 = iVar12 * 0x274 + DAT_00fe1fa8;
          uVar5 = FUN_0060a19c(iVar9 + 0x138,*(undefined2 *)(iVar9 + 0x168),
                               *(undefined2 *)(iVar9 + 0x16a));
          *(undefined1 *)(iVar12 * 0x274 + DAT_00fe1fa8 + 100) = uVar5;
          FUN_00649e08(iVar12,1);
          return iVar12;
        }
        iVar12 = iVar12 + 1;
        iVar7 = iVar7 + 0x274;
      } while (iVar12 < 0xc4);
    }
    else if (((DAT_00fe1f82 == '\0') && (*(char *)((int)param_1 + 0x11) == '\0')) &&
            ((iVar6 = FUN_00608330(&DAT_00fd67f4,5), iVar6 == 0 &&
             ((DAT_01033960 < iVar7 && (iVar6 = FUN_00656370(0x7b), iVar6 == 0)))))) {
      iVar9 = FUN_00685550(iVar10,iVar11,0x7b,0);
    }
    else {
      uVar3 = DAT_00fd67fc;
      uVar2 = DAT_00fd67f8;
      if (bVar1) {
        uVar8 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar8 = DAT_00fd6800 ^ (uVar8 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar8;
        DAT_00fd67fc = DAT_00fd6800;
        fVar14 = (float)VectorSignedToFloat(uVar8 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        if ((int)(fVar14 * DAT_006947f0 * DAT_006947ec) == 0) {
          iVar7 = 0;
          iVar9 = DAT_00fe1fa8;
          do {
            if (*(char *)(iVar9 + 0x100) == '\0') {
              if (0xc3 < iVar7) {
                DAT_00fd67f4 = uVar2;
                DAT_00fd67f8 = uVar3;
                DAT_00fd6800 = uVar8;
                return iVar7;
              }
              DAT_00fd6800 = uVar8;
              FUN_00684770(iVar7 * 0x274 + DAT_00fe1fa8,0x11f);
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              iVar10 = iVar10 - (uint)(*(ushort *)(iVar9 + 0x168) >> 1);
              *(int *)(iVar9 + 0x148) = iVar10;
              uVar13 = VectorSignedToFloat(iVar10,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar9 + 0x138) = uVar13;
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              iVar11 = iVar11 - (uint)*(ushort *)(iVar9 + 0x16a);
              *(int *)(iVar9 + 0x14c) = iVar11;
              uVar13 = VectorSignedToFloat(iVar11,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar9 + 0x13c) = uVar13;
              *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
              *(undefined4 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              uVar5 = FUN_0060a19c(iVar9 + 0x138,*(undefined2 *)(iVar9 + 0x168),
                                   *(undefined2 *)(iVar9 + 0x16a));
              *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 100) = uVar5;
              FUN_00649e08(iVar7,1);
              return iVar7;
            }
            iVar7 = iVar7 + 1;
            iVar9 = iVar9 + 0x274;
          } while (iVar7 < 0xc4);
          DAT_00fd67f4 = uVar2;
          DAT_00fd67f8 = uVar3;
          DAT_00fd6800 = uVar8;
          return 0xc4;
        }
        DAT_00fd6800 = uVar8;
        iVar6 = FUN_00608330(&DAT_00fd67f4,0x19);
        if (iVar6 == 0) {
          if ((iVar9 == 0) || (iVar7 = FUN_00608330(&DAT_00fd67f4,10), iVar7 == 0)) {
            iVar7 = 0;
            iVar9 = DAT_00fe1fa8;
            do {
              if (*(char *)(iVar9 + 0x100) == '\0') {
                if (0xc3 < iVar7) {
                  return iVar7;
                }
                FUN_00684770(iVar7 * 0x274 + DAT_00fe1fa8,0x125);
                iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
                iVar10 = iVar10 - (uint)(*(ushort *)(iVar9 + 0x168) >> 1);
                *(int *)(iVar9 + 0x148) = iVar10;
                uVar13 = VectorSignedToFloat(iVar10,(byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar9 + 0x138) = uVar13;
                iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
                iVar11 = iVar11 - (uint)*(ushort *)(iVar9 + 0x16a);
                *(int *)(iVar9 + 0x14c) = iVar11;
                uVar13 = VectorSignedToFloat(iVar11,(byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar9 + 0x13c) = uVar13;
                *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
                *(undefined4 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
                iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
                uVar5 = FUN_0060a19c(iVar9 + 0x138,*(undefined2 *)(iVar9 + 0x168),
                                     *(undefined2 *)(iVar9 + 0x16a));
                *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 100) = uVar5;
                FUN_00649e08(iVar7,1);
                return iVar7;
              }
              iVar7 = iVar7 + 1;
              iVar9 = iVar9 + 0x274;
            } while (iVar7 < 0xc4);
            return 0xc4;
          }
          if ((iVar9 != 1) && (iVar7 = FUN_00608330(&DAT_00fd67f4,10), iVar7 != 0)) {
            if ((iVar9 != 2) && (iVar9 = FUN_00608330(&DAT_00fd67f4,10), iVar9 != 0)) {
              return -1;
            }
            iVar7 = 0;
            iVar9 = DAT_00fe1fa8;
            do {
              if (*(char *)(iVar9 + 0x100) == '\0') {
                if (0xc3 < iVar7) {
                  return iVar7;
                }
                FUN_00684770(iVar7 * 0x274 + DAT_00fe1fa8,0x124);
                iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
                iVar10 = iVar10 - (uint)(*(ushort *)(iVar9 + 0x168) >> 1);
                *(int *)(iVar9 + 0x148) = iVar10;
                uVar13 = VectorSignedToFloat(iVar10,(byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar9 + 0x138) = uVar13;
                iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
                iVar11 = iVar11 - (uint)*(ushort *)(iVar9 + 0x16a);
                *(int *)(iVar9 + 0x14c) = iVar11;
                uVar13 = VectorSignedToFloat(iVar11,(byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar9 + 0x13c) = uVar13;
                *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
                *(undefined4 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
                iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
                uVar5 = FUN_0060a19c(iVar9 + 0x138,*(undefined2 *)(iVar9 + 0x168),
                                     *(undefined2 *)(iVar9 + 0x16a));
                *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 100) = uVar5;
                FUN_00649e08(iVar7,1);
                return iVar7;
              }
              iVar7 = iVar7 + 1;
              iVar9 = iVar9 + 0x274;
            } while (iVar7 < 0xc4);
            return 0xc4;
          }
          iVar7 = 0;
          iVar9 = DAT_00fe1fa8;
          do {
            if (*(char *)(iVar9 + 0x100) == '\0') {
              if (0xc3 < iVar7) {
                return iVar7;
              }
              FUN_00684770(iVar7 * 0x274 + DAT_00fe1fa8,0x123);
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              iVar10 = iVar10 - (uint)(*(ushort *)(iVar9 + 0x168) >> 1);
              *(int *)(iVar9 + 0x148) = iVar10;
              uVar13 = VectorSignedToFloat(iVar10,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar9 + 0x138) = uVar13;
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              iVar11 = iVar11 - (uint)*(ushort *)(iVar9 + 0x16a);
              *(int *)(iVar9 + 0x14c) = iVar11;
              uVar13 = VectorSignedToFloat(iVar11,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar9 + 0x13c) = uVar13;
              *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
              *(undefined4 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              uVar5 = FUN_0060a19c(iVar9 + 0x138,*(undefined2 *)(iVar9 + 0x168),
                                   *(undefined2 *)(iVar9 + 0x16a));
              *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 100) = uVar5;
              FUN_00649e08(iVar7,1);
              return iVar7;
            }
            iVar7 = iVar7 + 1;
            iVar9 = iVar9 + 0x274;
          } while (iVar7 < 0xc4);
          return 0xc4;
        }
      }
      cVar4 = DAT_01033966;
      if (bVar1) {
        if ((((DAT_01033966 != '\0') && (iVar6 = FUN_00656370(0x122), iVar6 == 0)) && (iVar9 == 0))
           && (iVar6 = FUN_00608330(&DAT_00fd67f4,0x2d), iVar6 == 0)) {
          iVar7 = 0;
          iVar9 = DAT_00fe1fa8;
          do {
            if (*(char *)(iVar9 + 0x100) == '\0') {
              if (0xc3 < iVar7) {
                return iVar7;
              }
              FUN_00684770(iVar7 * 0x274 + DAT_00fe1fa8,0x122);
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              iVar10 = iVar10 - (uint)(*(ushort *)(iVar9 + 0x168) >> 1);
              *(int *)(iVar9 + 0x148) = iVar10;
              uVar13 = VectorSignedToFloat(iVar10,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar9 + 0x138) = uVar13;
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              iVar11 = iVar11 - (uint)*(ushort *)(iVar9 + 0x16a);
              *(int *)(iVar9 + 0x14c) = iVar11;
              uVar13 = VectorSignedToFloat(iVar11,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar9 + 0x13c) = uVar13;
              *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
              *(undefined4 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
              iVar9 = iVar7 * 0x274 + DAT_00fe1fa8;
              uVar5 = FUN_0060a19c(iVar9 + 0x138,*(undefined2 *)(iVar9 + 0x168),
                                   *(undefined2 *)(iVar9 + 0x16a));
              *(undefined1 *)(iVar7 * 0x274 + DAT_00fe1fa8 + 100) = uVar5;
              FUN_00649e08(iVar7,1);
              return iVar7;
            }
            iVar7 = iVar7 + 1;
            iVar9 = iVar9 + 0x274;
          } while (iVar7 < 0xc4);
          return 0xc4;
        }
        if (((cVar4 != '\0') && ((iVar9 == 1 || (iVar9 == 2)))) &&
           (iVar6 = FUN_00608330(&DAT_00fd67f4,0x1e), iVar6 == 0)) {
          iVar9 = FUN_00685550(iVar10,iVar11,0x121,0);
          return iVar9;
        }
        iVar6 = FUN_00608330(&DAT_00fd67f4,0x14);
        if (iVar6 == 0) {
          iVar7 = 0x119;
          if (iVar9 == 0) {
            iVar7 = 0x11b;
          }
          else if (iVar9 == 2) {
            iVar7 = 0x11d;
          }
          iVar9 = FUN_00608330(&DAT_00fd67f4,2);
          iVar12 = FUN_00656370(iVar9 + iVar7);
          if (iVar12 != 0) {
            return -1;
          }
          iVar9 = FUN_00685550(iVar10,iVar11,iVar9 + iVar7,0);
          return iVar9;
        }
        iVar6 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar6 != 0) {
          iVar7 = 0x10d;
          if (iVar9 == 0) {
            iVar7 = 0x111;
          }
          else if (iVar9 == 2) {
            iVar7 = 0x115;
          }
          iVar9 = FUN_00608330(&DAT_00fd67f4,4);
          iVar9 = FUN_00685550(iVar10,iVar11,iVar9 + iVar7,0);
          return iVar9;
        }
      }
      iVar6 = FUN_00608330(&DAT_00fd67f4,0x25);
      if (iVar6 == 0) {
        iVar9 = FUN_00685550(iVar10,iVar11,0x47,0);
      }
      else {
        if (iVar9 == 1) {
          iVar9 = FUN_00608330(&DAT_00fd67f4,4);
          if ((iVar9 == 0) && (iVar9 = FUN_0065aaa8(iVar12,iVar7), iVar9 == 0)) {
            iVar9 = FUN_00685550(iVar10,iVar11,0x46,0);
            return iVar9;
          }
        }
        else if ((iVar9 == 2) && (iVar9 = FUN_00608330(&DAT_00fd67f4,0xf), iVar9 == 0)) {
          iVar9 = FUN_00685550(iVar10,iVar11,0x48,0);
          return iVar9;
        }
        iVar9 = FUN_00608330(&DAT_00fd67f4,9);
        if (iVar9 == 0) {
          iVar9 = FUN_00608330(&DAT_01070468,4);
          if (iVar9 == 0) {
            uVar13 = 0x3f4;
          }
          else {
            uVar13 = 0x22;
          }
          iVar9 = FUN_00685550(iVar10,iVar11,uVar13,0);
        }
        else {
          iVar9 = FUN_00608330(&DAT_00fd67f4,7);
          if (iVar9 == 0) {
            iVar9 = FUN_00685550(iVar10,iVar11,0x20,0);
          }
          else {
            iVar9 = FUN_00608330(&DAT_00fd67f4,5);
            if (iVar9 == 0) {
              iVar9 = FUN_00685550(iVar10,iVar11,0x126,0);
            }
            else if (iVar9 == 1) {
              iVar9 = FUN_00685550(iVar10,iVar11,0x127,0);
            }
            else if (iVar9 == 2) {
              iVar9 = FUN_00685550(iVar10,iVar11,0x128,0);
            }
            else {
              iVar9 = FUN_00685550(iVar10,iVar11,0x1f,0);
              iVar7 = FUN_00608330(&DAT_00fd67f4,4);
              if (iVar7 == 0) {
                FUN_007ab078(auStack_58,"Big Boned");
                FUN_0067a84c(iVar9 * 0x274 + DAT_00fe1fa8,auStack_58);
              }
              else {
                iVar7 = FUN_00608330(&DAT_00fd67f4,5);
                if (iVar7 != 0) {
                  return iVar9;
                }
                FUN_007ab078(auStack_58,"Short Bones");
                FUN_0067a84c(iVar9 * 0x274 + DAT_00fe1fa8,auStack_58);
              }
              FUN_007aa754(auStack_58);
            }
          }
        }
      }
    }
  }
  return iVar9;
}

