
void FUN_00783b20(int param_1,int param_2,int param_3)

{
  bool bVar1;
  short *psVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  short sVar8;
  short sVar9;
  uint uVar10;
  int iVar11;
  int iVar12;
  uint in_fpscr;
  float fVar13;
  undefined4 uVar14;
  undefined4 uVar15;
  float fVar16;
  undefined4 uVar17;
  short local_50 [8];
  short local_40 [14];
  
  iVar5 = 0;
  if (0 < DAT_0102a744) {
    psVar2 = &DAT_0102e668;
    do {
      if ((*psVar2 == param_1) && (psVar2[1] == param_2)) {
        return;
      }
      iVar5 = iVar5 + 1;
      psVar2 = psVar2 + 2;
    } while (iVar5 < DAT_0102a744);
  }
  sVar9 = *(short *)(param_3 + 6);
  if (((*(byte *)(param_3 + 1) & 0x80) != 0) &&
     (((sVar9 != 0xe2 || (param_2 <= DAT_00902ebc)) || (DAT_00fe1f8d != '\0')))) {
    if ((*(byte *)(param_3 + 1) & 2) == 0) {
      iVar5 = FUN_0073103c((DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928);
      if ((iVar5 != 0) && (FUN_007729e8(param_1,param_2,0), DAT_00fe1ec4 != 1)) {
        FUN_00649a94(param_1,param_2,0,0);
      }
    }
    else {
      FUN_007746dc(param_1,param_2);
    }
  }
  if ((*(byte *)(param_3 + 1) & 1) == 0) {
    return;
  }
  if (sVar9 == 0x90) {
    FUN_00772d30(param_1,param_2);
  }
  else {
    if ((sVar9 != 0x82) && (sVar9 != 0x83)) {
      if (sVar9 == 0xb) {
        FUN_00772624(param_1,param_2,1);
        FUN_006494f4(param_1,param_2);
        return;
      }
      if (sVar9 == 10) {
        iVar5 = FUN_00608330(&DAT_00fd67f4,2);
        iVar5 = FUN_00776dc8(param_1,param_2,iVar5 * 2 + -1);
        if (iVar5 == 0) {
          return;
        }
        FUN_00648da4(param_1,param_2,iVar5);
        return;
      }
      if (sVar9 == 0xd8) {
        FUN_007523e8(param_1,param_2);
        FUN_0074a708(param_1,param_2);
        return;
      }
      if (sVar9 == 0xeb) {
        iVar5 = (int)((ulonglong)((longlong)(int)*(short *)(param_3 + 10) * (longlong)DAT_00784364)
                     >> 0x20);
        param_1 = param_1 - ((iVar5 >> 2) - (iVar5 >> 0x1f));
        if (((*(char *)(param_3 + 8) == 'W') && (DAT_00902ebc < param_2)) && (DAT_00fe1f8d == '\0'))
        {
          return;
        }
        uVar10 = in_fpscr & 0xfffffff;
        if (DAT_01033500 < 0.0) {
          DAT_01033500 = (float)VectorSignedToFloat(param_1,(byte)(uVar10 >> 0x16) & 3);
          DAT_01033504 = (float)VectorSignedToFloat(param_2,(byte)(uVar10 >> 0x16) & 3);
          if ((*(byte *)(param_3 + 1) & 4) == 0) {
            return;
          }
          DAT_01033504 = DAT_01033504 + 0.5;
          return;
        }
        fVar16 = (float)VectorSignedToFloat(param_1,(byte)(uVar10 >> 0x16) & 3);
        if ((DAT_01033500 == fVar16) &&
           (fVar13 = (float)VectorSignedToFloat(param_2,(byte)(uVar10 >> 0x16) & 3),
           DAT_01033504 == fVar13)) {
          return;
        }
        DAT_01033508 = fVar16;
        DAT_0103350c = (float)VectorSignedToFloat(param_2,(byte)(uVar10 >> 0x16) & 3);
        if ((*(byte *)(param_3 + 1) & 4) == 0) {
          return;
        }
        DAT_0103350c = DAT_0103350c + 0.5;
        return;
      }
      if (sVar9 == 4) {
        sVar9 = *(short *)(param_3 + 10);
        if (sVar9 < 0x42) {
          *(short *)(param_3 + 10) = sVar9 + 0x42;
        }
        else {
          *(short *)(param_3 + 10) = sVar9 + -0x42;
        }
      }
      else {
        if (sVar9 != 0x95) {
          if (sVar9 == 0xf4) {
            iVar5 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(param_3 + 10) * (longlong)DAT_00784364) >> 0x20
                         );
            iVar5 = (iVar5 >> 2) - (iVar5 >> 0x1f);
            iVar6 = (int)((ulonglong)((longlong)iVar5 * (longlong)DAT_00784368) >> 0x20);
            param_1 = param_1 + ((iVar6 - (iVar6 >> 0x1f)) * 3 - iVar5);
            iVar5 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(param_3 + 0xc) * (longlong)DAT_00784364) >>
                         0x20);
            iVar5 = (iVar5 >> 2) - (iVar5 >> 0x1f);
            iVar6 = (int)((ulonglong)((longlong)iVar5 * (longlong)DAT_00784368) >> 0x20);
            param_2 = param_2 - (iVar5 + (iVar6 - (iVar6 >> 0x1f)) * -3);
            sVar9 = 0x36;
            if (0x35 < *(short *)(param_3 + 10)) {
              sVar9 = -0x36;
            }
            iVar5 = param_1 + 3;
            if (iVar5 <= param_1) {
              return;
            }
            iVar6 = param_2;
            do {
              for (; iVar6 < param_2 + 2; iVar6 = iVar6 + 1) {
                FUN_0074a708(param_1,iVar6);
                iVar3 = (DAT_00902934 * param_1 + iVar6) * 0xe + DAT_00902928;
                *(short *)(iVar3 + 10) = *(short *)(iVar3 + 10) + sVar9;
              }
              param_1 = param_1 + 1;
              iVar6 = param_2;
            } while (param_1 < iVar5);
            return;
          }
          if (sVar9 == 0x2a) {
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            iVar6 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_00784364) >> 0x20)
            ;
            iVar6 = param_2 - ((iVar6 >> 2) - (iVar6 >> 0x1f) & 1U);
            sVar9 = 0x12;
            if (0 < *(short *)(iVar5 + 10)) {
              sVar9 = -0x12;
            }
            iVar5 = (DAT_00902934 * param_1 + iVar6) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 10) = *(short *)(iVar5 + 10) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + iVar6) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x18) = *(short *)(iVar5 + 0x18) + sVar9;
            FUN_0074a708(param_1,iVar6);
            FUN_0074a708(param_1,iVar6 + 1);
            FUN_006496ec(param_1,param_2,2);
            return;
          }
          if (sVar9 == 0x5d) {
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            iVar6 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_00784364) >> 0x20)
            ;
            param_2 = param_2 - ((iVar6 >> 2) - (iVar6 >> 0x1f));
            sVar9 = 0x12;
            if (0 < *(short *)(iVar5 + 10)) {
              sVar9 = -0x12;
            }
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 10) = *(short *)(iVar5 + 10) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x18) = *(short *)(iVar5 + 0x18) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x26) = *(short *)(iVar5 + 0x26) + sVar9;
            FUN_0074a708(param_1,param_2);
            FUN_0074a708(param_1,param_2 + 1);
            FUN_0074a708(param_1,param_2 + 2);
            FUN_006496ec(param_1,param_2 + 1,3);
            return;
          }
          if (((sVar9 == 0xad) || (sVar9 == 0x7e)) || ((sVar9 == 100 || (sVar9 == 0x5f)))) {
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            iVar6 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_00784c10) >> 0x20)
            ;
            param_2 = param_2 - ((iVar6 >> 2) - (iVar6 >> 0x1f));
            iVar5 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 10) * (longlong)DAT_00784c10) >> 0x20);
            param_1 = param_1 - ((iVar5 >> 2) - (iVar5 >> 0x1f) & 1U);
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            sVar9 = 0x24;
            if (0 < *(short *)(iVar5 + 10)) {
              sVar9 = -0x24;
            }
            iVar6 = param_1 + 1;
            *(short *)(iVar5 + 10) = *(short *)(iVar5 + 10) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x18) = *(short *)(iVar5 + 0x18) + sVar9;
            iVar5 = (iVar6 * DAT_00902934 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 10) = *(short *)(iVar5 + 10) + sVar9;
            iVar5 = (iVar6 * DAT_00902934 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x18) = *(short *)(iVar5 + 0x18) + sVar9;
            FUN_0074a708(param_1,param_2);
            FUN_0074a708(param_1,param_2 + 1);
            FUN_0074a708(iVar6,param_2);
            FUN_0074a708(iVar6,param_2 + 1);
            FUN_006496ec(param_1,param_2,3);
            return;
          }
          if (sVar9 == 0x22) {
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            iVar6 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_00784364) >> 0x20)
            ;
            iVar6 = (iVar6 >> 2) - (iVar6 >> 0x1f);
            iVar3 = (int)((ulonglong)((longlong)iVar6 * (longlong)DAT_00784368) >> 0x20);
            param_2 = param_2 - (iVar6 + (iVar3 - (iVar3 >> 0x1f)) * -3);
            iVar5 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 10) * (longlong)DAT_00784364) >> 0x20);
            iVar5 = (iVar5 >> 2) - (iVar5 >> 0x1f);
            iVar6 = (int)((ulonglong)((longlong)iVar5 * (longlong)DAT_00784368) >> 0x20);
            param_1 = param_1 + ((iVar6 - (iVar6 >> 0x1f)) * 3 - iVar5);
            sVar9 = 0x36;
            if (0 < *(short *)((DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928 + 10)) {
              sVar9 = -0x36;
            }
            if (param_1 < param_1 + 3) {
              iVar5 = param_2;
              iVar6 = DAT_00902928;
              iVar3 = DAT_00902934;
              iVar12 = param_1;
              do {
                for (; iVar5 < param_2 + 3; iVar5 = iVar5 + 1) {
                  iVar6 = (iVar3 * iVar12 + iVar5) * 0xe + iVar6;
                  *(short *)(iVar6 + 10) = *(short *)(iVar6 + 10) + sVar9;
                  FUN_0074a708(iVar12,iVar5);
                  iVar6 = DAT_00902928;
                  iVar3 = DAT_00902934;
                }
                iVar12 = iVar12 + 1;
                iVar5 = param_2;
              } while (iVar12 < param_1 + 3);
            }
            FUN_006496ec(param_1 + 1,param_2 + 1,3);
            return;
          }
          if ((sVar9 == 0x21) || (sVar9 == 0xae)) {
            sVar9 = 0x12;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            if (0 < *(short *)(iVar5 + 10)) {
              sVar9 = -0x12;
            }
            *(short *)(iVar5 + 10) = *(short *)(iVar5 + 10) + sVar9;
            FUN_006496ec(param_1,param_2,3);
            return;
          }
          if (sVar9 == 0x5c) {
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            iVar6 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_00784364) >> 0x20)
            ;
            param_2 = param_2 - ((iVar6 >> 2) - (iVar6 >> 0x1f));
            sVar9 = 0x12;
            if (0 < *(short *)(iVar5 + 10)) {
              sVar9 = -0x12;
            }
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 10) = *(short *)(iVar5 + 10) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x18) = *(short *)(iVar5 + 0x18) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x26) = *(short *)(iVar5 + 0x26) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x34) = *(short *)(iVar5 + 0x34) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x42) = *(short *)(iVar5 + 0x42) + sVar9;
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            *(short *)(iVar5 + 0x50) = *(short *)(iVar5 + 0x50) + sVar9;
            FUN_0074a708(param_1,param_2);
            FUN_0074a708(param_1,param_2 + 1);
            FUN_0074a708(param_1,param_2 + 2);
            FUN_0074a708(param_1,param_2 + 3);
            FUN_0074a708(param_1,param_2 + 4);
            FUN_0074a708(param_1,param_2 + 5);
            FUN_006496ec(param_1,param_2 + 3,7);
            return;
          }
          if (sVar9 == 0x89) {
            iVar5 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(param_3 + 0xc) * (longlong)DAT_00784364) >>
                         0x20);
            switch((iVar5 >> 2) - (iVar5 >> 0x1f)) {
            case 0:
              goto switchD_0078416e_caseD_0;
            case 1:
              iVar5 = FUN_0074a6a8(param_1,param_2,0xb4);
              if (iVar5 == 0) {
                return;
              }
              if (*(short *)(param_3 + 10) == 0) {
                iVar5 = -1;
              }
              else {
                iVar5 = 1;
              }
              uVar17 = VectorSignedToFloat(iVar5 * 0xc,(byte)(in_fpscr >> 0x16) & 3);
              uVar15 = VectorSignedToFloat(param_2 * 0x10 + 9,(byte)(in_fpscr >> 0x16) & 3);
              uVar14 = VectorSignedToFloat(param_1 * 0x10 + iVar5 * 10 + 8,
                                           (byte)(in_fpscr >> 0x16) & 3);
              FUN_006f7004(uVar14,uVar15,uVar17,DAT_00784360,0x40000000,DAT_00784360,DAT_00784360,
                           0xb8,0x28,4);
              return;
            case 2:
              iVar5 = FUN_0074a6a8(param_1,param_2,0xb4);
              if (iVar5 == 0) {
                return;
              }
              if (*(short *)(param_3 + 10) == 0) {
                iVar5 = -1;
              }
              else {
                iVar5 = 1;
              }
              uVar17 = VectorSignedToFloat(iVar5 * 0xc,(byte)(in_fpscr >> 0x16) & 3);
              uVar15 = VectorSignedToFloat(param_2 * 0x10 + 9,(byte)(in_fpscr >> 0x16) & 3);
              uVar14 = VectorSignedToFloat(param_1 * 0x10 + iVar5 * 10 + 8,
                                           (byte)(in_fpscr >> 0x16) & 3);
              FUN_006f7004(uVar14,uVar15,uVar17,DAT_00784360,0x40000000,DAT_00784360,DAT_00784360,
                           0xbb,0x28,4);
              return;
            case 3:
              iVar5 = FUN_0074a6a8(param_1,param_2,0xf0);
              if (iVar5 == 0) {
                return;
              }
              uVar14 = FUN_005c4e6c(&DAT_00fd67f4,0xffffffec,0x15);
              uVar15 = FUN_005c4e6c(&DAT_00fd67f4,0,0x15);
              fVar16 = (float)VectorSignedToFloat(uVar14,(byte)(in_fpscr >> 0x16) & 3);
              fVar13 = (float)VectorSignedToFloat(uVar15,(byte)(in_fpscr >> 0x16) & 3);
              uVar15 = VectorSignedToFloat(param_2 * 0x10 + 0x16,(byte)(in_fpscr >> 0x16) & 3);
              uVar14 = VectorSignedToFloat(param_1 * 0x10 + 8,(byte)(in_fpscr >> 0x16) & 3);
              FUN_006f7004(uVar14,uVar15,fVar16 * DAT_0078435c,fVar13 * DAT_0078435c + 4.0,
                           0x40000000,DAT_00784360,DAT_00784360,0xb9,0x28,4);
              return;
            default:
              iVar5 = FUN_0074a6a8(param_1,param_2,0x5a);
              if (iVar5 == 0) {
                return;
              }
              uVar15 = VectorSignedToFloat(param_2 * 0x10 + 0x1a,(byte)(in_fpscr >> 0x16) & 3);
              uVar14 = VectorSignedToFloat(param_1 * 0x10 + 8,(byte)(in_fpscr >> 0x16) & 3);
              FUN_006f7004(uVar14,uVar15,DAT_007846e8,0x41000000,0x40000000,DAT_007846e8,
                           DAT_007846e8,0xba,0x3c,4);
              return;
            }
          }
          if ((sVar9 == 0x8b) || (sVar9 == 0x23)) {
            FUN_0075425c(param_1,param_2);
            return;
          }
          if (sVar9 == 0xcf) {
            FUN_00754178(param_1,param_2);
            return;
          }
          if (sVar9 == 0xd2) {
            FUN_00777104(param_1,param_2);
            return;
          }
          if (sVar9 == 0x8d) {
            FUN_007743f8(param_1,param_2,0,0,1,0);
            FUN_00649a94(param_1,param_2,0,0);
            uVar15 = VectorSignedToFloat(param_2 * 0x10 + 8,(byte)(in_fpscr >> 0x16) & 3);
            uVar14 = VectorSignedToFloat(param_1 * 0x10 + 8,(byte)(in_fpscr >> 0x16) & 3);
            FUN_006f7004(uVar14,uVar15,DAT_007846e8,DAT_007846e8,0x41200000,DAT_007846e8,
                         DAT_007846e8,0x6c,0xfa,4);
            return;
          }
          if ((sVar9 == 0x8e) || (sVar9 == 0x8f)) {
            iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
            iVar6 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_00784c10) >> 0x20)
            ;
            param_2 = param_2 - ((iVar6 >> 2) - (iVar6 >> 0x1f));
            iVar5 = (int)((ulonglong)
                          ((longlong)(int)*(short *)(iVar5 + 10) * (longlong)DAT_00784c10) >> 0x20);
            iVar5 = (iVar5 >> 2) - (iVar5 >> 0x1f);
            if (1 < iVar5) {
              iVar5 = iVar5 + -2;
            }
            param_1 = param_1 - iVar5;
            FUN_0074a708(param_1,param_2);
            FUN_0074a708(param_1,param_2 + 1);
            FUN_0074a708(param_1 + 1,param_2);
            FUN_0074a708(param_1 + 1,param_2 + 1);
            sVar8 = (short)param_1;
            if (sVar9 == 0x8e) {
              iVar5 = 0;
              psVar2 = &DAT_010305a8 + DAT_0102a748 * 2;
              do {
                if (0x12 < DAT_0102a748) {
                  return;
                }
                sVar9 = sVar8;
                if (iVar5 == 0) {
                  iVar6 = param_2 + 1;
                }
                else if (iVar5 == 1) {
                  iVar6 = param_2 + 1;
                  sVar9 = sVar8 + 1;
                }
                else {
                  iVar6 = param_2;
                  if (iVar5 != 2) {
                    sVar9 = sVar8 + 1;
                  }
                }
                *psVar2 = sVar9;
                iVar5 = iVar5 + 1;
                DAT_0102a748 = DAT_0102a748 + 1;
                psVar2[1] = (short)iVar6;
                psVar2 = psVar2 + 2;
              } while (iVar5 < 4);
              return;
            }
            iVar5 = 0;
            psVar2 = &DAT_010305f8 + DAT_0102a74c * 2;
            do {
              if (0x12 < DAT_0102a74c) {
                return;
              }
              sVar9 = sVar8;
              if (iVar5 == 0) {
                iVar6 = param_2 + 1;
              }
              else if (iVar5 == 1) {
                iVar6 = param_2 + 1;
                sVar9 = sVar8 + 1;
              }
              else {
                iVar6 = param_2;
                if (iVar5 != 2) {
                  sVar9 = sVar8 + 1;
                }
              }
              *psVar2 = sVar9;
              iVar5 = iVar5 + 1;
              DAT_0102a74c = DAT_0102a74c + 1;
              psVar2[1] = (short)iVar6;
              psVar2 = psVar2 + 2;
            } while (iVar5 < 4);
            return;
          }
          if (sVar9 != 0x69) {
            return;
          }
          iVar5 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
          iVar6 = (int)((ulonglong)((longlong)(int)*(short *)(iVar5 + 0xc) * (longlong)DAT_007846ec)
                       >> 0x20);
          iVar6 = param_2 - ((iVar6 >> 2) - (iVar6 >> 0x1f));
          iVar5 = (int)((ulonglong)((longlong)(int)*(short *)(iVar5 + 10) * (longlong)DAT_007846ec)
                       >> 0x20);
          uVar10 = (iVar5 >> 2) - (iVar5 >> 0x1f);
          iVar3 = (int)uVar10 >> 1;
          iVar5 = param_1 - (uVar10 & 1);
          FUN_0074a708(iVar5,iVar6);
          FUN_0074a708(iVar5,iVar6 + 1);
          FUN_0074a708(iVar5,iVar6 + 2);
          FUN_0074a708(iVar5 + 1,iVar6);
          FUN_0074a708(iVar5 + 1,iVar6 + 1);
          FUN_0074a708(iVar5 + 1,iVar6 + 2);
          iVar5 = (iVar5 + 1) * 0x10;
          iVar6 = (iVar6 + 3) * 0x10;
          if (iVar3 == 4) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,1);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 1;
          }
          else if (iVar3 == 7) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x31);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -6;
            iVar5 = iVar5 + -4;
            uVar14 = 0x31;
          }
          else if (iVar3 == 8) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x37);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 0x37;
          }
          else if (iVar3 == 9) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x2e);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 0x2e;
          }
          else if (iVar3 == 10) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x15);
            if (iVar3 == 0) {
              return;
            }
            uVar14 = 0x15;
          }
          else if (iVar3 == 0x12) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x43);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 0x43;
          }
          else if (iVar3 == 0x17) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x3f);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 0x3f;
          }
          else if (iVar3 == 0x1b) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x55);
            if (iVar3 == 0) {
              return;
            }
            iVar5 = iVar5 + -9;
            uVar14 = 0x55;
          }
          else if (iVar3 == 0x1c) {
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x4a);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 0x4a;
          }
          else {
            if (iVar3 != 0x2a) {
              if (iVar3 == 0x25) {
                iVar3 = FUN_0074a6a8(param_1,param_2,600);
                if (iVar3 == 0) {
                  return;
                }
                iVar3 = FUN_006138d0(iVar5,iVar6,0x3a);
                if (iVar3 == 0) {
                  return;
                }
                iVar3 = FUN_006138d0(iVar5,iVar6,0x6c6);
                if (iVar3 == 0) {
                  return;
                }
                FUN_00631590(iVar5,iVar6 + -0x10,0,0,0x3a,1,0,0,0);
                return;
              }
              if (iVar3 == 2) {
                iVar3 = FUN_0074a6a8(param_1,param_2,600);
                if (iVar3 == 0) {
                  return;
                }
                iVar3 = FUN_006138d0(iVar5,iVar6,0xb8);
                if (iVar3 == 0) {
                  return;
                }
                iVar3 = FUN_006138d0(iVar5,iVar6,0x6c7);
                if (iVar3 == 0) {
                  return;
                }
                FUN_00631590(iVar5,iVar6 + -0x10,0,0,0xb8,1,0,0,0);
                return;
              }
              if (iVar3 == 0x11) {
                iVar3 = FUN_0074a6a8(param_1,param_2,600);
                if (iVar3 == 0) {
                  return;
                }
                iVar3 = FUN_006138d0(iVar5,iVar6,0xa6);
                if (iVar3 == 0) {
                  return;
                }
                FUN_00631590(iVar5,iVar6 + -0x14,0,0,0xa6,1,0,0,0);
                return;
              }
              if (iVar3 != 0x28) {
                if (iVar3 != 0x29) {
                  return;
                }
                iVar12 = FUN_0074a6a8(param_1,param_2,300);
                iVar3 = DAT_00fe1fa8;
                if (iVar12 == 0) {
                  return;
                }
                iVar4 = 0;
                iVar7 = 0xc3;
                psVar2 = local_50;
                iVar12 = DAT_00fe1fa8 + DAT_00784c14;
                do {
                  if ((*(char *)(iVar12 + 0x100) != '\0') &&
                     ((((iVar11 = *(int *)(iVar12 + 0x104), iVar11 == 0x12 || (iVar11 == 0x14)) ||
                       (iVar11 == 0x7c)) || ((iVar11 == 0xb2 || (iVar11 == 0xd1)))))) {
                    *psVar2 = (short)iVar7;
                    iVar4 = iVar4 + 1;
                    psVar2 = psVar2 + 1;
                    if (iVar4 == 6) break;
                  }
                  iVar7 = iVar7 + -1;
                  iVar12 = iVar12 + -0x274;
                } while (-1 < iVar7);
                if (iVar4 < 1) {
                  return;
                }
                iVar12 = FUN_00608330(&DAT_00fd67f4);
                iVar12 = (int)local_50[iVar12];
                iVar3 = iVar12 * 0x274 + iVar3;
                *(uint *)(iVar3 + 0x148) = iVar5 - (uint)(*(ushort *)(iVar3 + 0x168) >> 1);
                iVar5 = iVar12 * 0x274 + DAT_00fe1fa8;
                *(uint *)(iVar5 + 0x14c) = (iVar6 - (uint)*(ushort *)(iVar5 + 0x16a)) + -1;
                iVar5 = iVar12 * 0x274 + DAT_00fe1fa8;
                uVar14 = VectorSignedToFloat(*(undefined4 *)(iVar5 + 0x148),
                                             (byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar5 + 0x138) = uVar14;
                iVar5 = iVar12 * 0x274 + DAT_00fe1fa8;
                uVar14 = VectorSignedToFloat(*(undefined4 *)(iVar5 + 0x14c),
                                             (byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar5 + 0x13c) = uVar14;
                FUN_00649e80(iVar12,0);
                return;
              }
              iVar12 = FUN_0074a6a8(param_1,param_2,300);
              iVar3 = DAT_00fe1fa8;
              if (iVar12 == 0) {
                return;
              }
              iVar4 = 0;
              iVar7 = 0xc3;
              psVar2 = local_40;
              iVar12 = DAT_00fe1fa8 + DAT_00784c14;
              do {
                if ((*(char *)(iVar12 + 0x100) != '\0') &&
                   (((((iVar11 = *(int *)(iVar12 + 0x104), iVar11 == 0x11 || (iVar11 == 0x13)) ||
                      ((iVar11 == 0x16 ||
                       (((iVar11 == 0x26 || (iVar11 == 0x36)) || (iVar11 == 0x6b)))))) ||
                     ((iVar11 == 0x6c || (iVar11 == 0x8e)))) ||
                    ((iVar11 == 0xa0 ||
                     (((iVar11 == 0xd0 || (iVar11 == 0xcf)) ||
                      ((iVar11 == 0xe4 || ((iVar11 == 0xe3 || (iVar11 == 0xe5)))))))))))) {
                  *psVar2 = (short)iVar7;
                  iVar4 = iVar4 + 1;
                  psVar2 = psVar2 + 1;
                  if (iVar4 == 0xe) break;
                }
                iVar7 = iVar7 + -1;
                iVar12 = iVar12 + -0x274;
              } while (-1 < iVar7);
              if (iVar4 < 1) {
                return;
              }
              iVar12 = FUN_00608330(&DAT_00fd67f4);
              iVar12 = (int)local_40[iVar12];
              iVar3 = iVar12 * 0x274 + iVar3;
              *(uint *)(iVar3 + 0x148) = iVar5 - (uint)(*(ushort *)(iVar3 + 0x168) >> 1);
              iVar5 = iVar12 * 0x274 + DAT_00fe1fa8;
              *(uint *)(iVar5 + 0x14c) = (iVar6 - (uint)*(ushort *)(iVar5 + 0x16a)) + -1;
              iVar5 = iVar12 * 0x274 + DAT_00fe1fa8;
              uVar14 = VectorSignedToFloat(*(undefined4 *)(iVar5 + 0x148),
                                           (byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar5 + 0x138) = uVar14;
              iVar5 = iVar12 * 0x274 + DAT_00fe1fa8;
              uVar14 = VectorSignedToFloat(*(undefined4 *)(iVar5 + 0x14c),
                                           (byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar5 + 0x13c) = uVar14;
              FUN_00649e80(iVar12,0);
              return;
            }
            iVar3 = FUN_0074a6a8(param_1,param_2,0x1e);
            if (iVar3 == 0) {
              return;
            }
            iVar3 = FUN_006563e8(iVar5,iVar6,0x3a);
            if (iVar3 == 0) {
              return;
            }
            iVar6 = iVar6 + -0xc;
            uVar14 = 0x3a;
          }
          iVar5 = FUN_00685550(iVar5,iVar6,uVar14,0);
          uVar14 = DAT_007846e8;
          if (iVar5 < 0) {
            return;
          }
          *(undefined4 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 0x1ec) = DAT_007846e8;
          *(undefined4 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 0x60) = uVar14;
          return;
        }
        sVar9 = *(short *)(param_3 + 10);
        if (sVar9 < 0x36) {
          *(short *)(param_3 + 10) = sVar9 + 0x36;
        }
        else {
          *(short *)(param_3 + 10) = sVar9 + -0x36;
        }
      }
      goto LAB_00784bfe;
    }
    if ((*(short *)(param_3 + -8) == 0x15) && ((*(byte *)(param_3 + -0xd) & 1) != 0)) {
      bVar1 = true;
    }
    else {
      bVar1 = false;
    }
    if (bVar1) {
      return;
    }
    *(ushort *)(param_3 + 6) = *(ushort *)(param_3 + 6) ^ 1;
  }
  FUN_007729e8(param_1,param_2,0xffffffff);
LAB_00784bfe:
  FUN_00649a94(param_1,param_2,0,0);
  return;
switchD_0078416e_caseD_0:
  iVar5 = FUN_0074a6a8(param_1,param_2,0xb4);
  if (iVar5 == 0) {
    return;
  }
  if (*(short *)(param_3 + 10) == 0) {
    iVar5 = -1;
  }
  else {
    iVar5 = 1;
  }
  uVar17 = VectorSignedToFloat(iVar5 * 0xc,(byte)(in_fpscr >> 0x16) & 3);
  uVar15 = VectorSignedToFloat(param_2 * 0x10 + 9,(byte)(in_fpscr >> 0x16) & 3);
  uVar14 = VectorSignedToFloat(param_1 * 0x10 + iVar5 * 10 + 8,(byte)(in_fpscr >> 0x16) & 3);
  FUN_006f7004(uVar14,uVar15,uVar17,DAT_00784360,0x40000000,DAT_00784360,DAT_00784360,0x62,0x14,4);
  return;
}

