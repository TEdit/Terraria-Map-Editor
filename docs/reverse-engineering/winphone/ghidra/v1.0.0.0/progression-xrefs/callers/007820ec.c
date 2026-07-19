
void FUN_007820ec(int param_1,int param_2)

{
  ushort uVar1;
  short sVar2;
  bool bVar3;
  float fVar4;
  int iVar5;
  int iVar6;
  int iVar7;
  uint uVar8;
  int iVar9;
  uint uVar10;
  uint uVar11;
  undefined2 uVar12;
  int iVar13;
  int iVar14;
  int iVar15;
  uint uVar16;
  int iVar17;
  int iVar18;
  int iVar19;
  uint in_fpscr;
  float fVar20;
  int local_e8;
  int local_e4;
  int local_e0;
  uint local_dc;
  int local_d8;
  undefined4 local_d0;
  undefined4 uStack_cc;
  undefined1 auStack_c8 [48];
  undefined1 auStack_98 [48];
  undefined1 auStack_68 [68];
  
  iVar17 = DAT_00902934;
  iVar5 = DAT_00902928;
  local_d0 = 0xfffffffe;
  uStack_cc = 0xfffffffe;
  local_e0 = (DAT_00902934 * param_1 + param_2) * 0xe + DAT_00902928;
  if ((*(byte *)(local_e0 + 1) & 2) != 0) {
    return;
  }
  uVar1 = *(ushort *)(local_e0 + 6);
  local_e8 = param_1;
  local_d8 = param_2;
  if (uVar1 < 0xa5) {
    if (uVar1 == 0xa4) {
LAB_00782236:
      if ((DAT_01033960 < param_2) && (iVar7 = FUN_00608330(&DAT_00fd67f4,0x6e), iVar7 == 0)) {
        iVar14 = FUN_00608330(&DAT_00fd67f4,4);
        iVar7 = -1;
        iVar18 = 0;
        if (iVar14 != 0) {
          if (iVar14 == 1) {
            iVar7 = 1;
          }
          else {
            iVar7 = 0;
            iVar18 = -1;
            if (iVar14 != 2) {
              iVar18 = 1;
            }
          }
        }
        iVar18 = iVar18 + param_2;
        iVar7 = iVar7 + local_e8;
        local_e4 = iVar18;
        local_dc = iVar7;
        if ((*(byte *)((iVar7 * iVar17 + iVar18) * 0xe + iVar5 + 1) & 1) == 0) {
          iVar13 = local_e8 + -6;
          iVar14 = 0;
          if (iVar13 <= local_e8 + 6) {
            iVar15 = iVar17 * iVar13;
            iVar19 = param_2 + -6;
            iVar13 = ((local_e8 + 6) - iVar13) + 1;
            do {
              if (iVar19 <= param_2 + 6) {
                iVar9 = (iVar15 + iVar19) * 0xe + iVar5;
                iVar6 = ((param_2 + 6) - iVar19) + 1;
                do {
                  if ((*(short *)(iVar9 + 6) == 0x81) && ((*(byte *)(iVar9 + 1) & 1) != 0)) {
                    bVar3 = true;
                  }
                  else {
                    bVar3 = false;
                  }
                  if (bVar3) {
                    iVar14 = iVar14 + 1;
                  }
                  iVar9 = iVar9 + 0xe;
                  iVar6 = iVar6 + -1;
                } while (iVar6 != 0);
              }
              iVar15 = iVar15 + iVar17;
              iVar13 = iVar13 + -1;
            } while (iVar13 != 0);
            if (2 < iVar14) goto LAB_00782402;
          }
          FUN_007749e4(iVar7,iVar18,0x81,1,0,0xffffffff,0);
          FUN_00649a94(iVar7,iVar18,0,0);
          iVar17 = DAT_00902934;
          iVar5 = DAT_00902928;
        }
      }
    }
    else if (uVar1 == 0x3c) {
      if ((DAT_01033960 + DAT_00902ebc >> 1 < param_2) &&
         (iVar7 = FUN_00608330(&DAT_00fd67f4,200), iVar7 == 0)) {
        iVar7 = FUN_005c4e6c(&DAT_00fd67f4,0xfffffff6,0xb);
        iVar7 = iVar7 + local_e8;
        iVar18 = FUN_005c4e6c(&DAT_00fd67f4,0xfffffff6,0xb);
        param_2 = param_2 + iVar18;
        iVar18 = (iVar17 * iVar7 + param_2) * 0xe + iVar5;
        if ((*(short *)(iVar18 + 6) == 0x3b) && ((*(byte *)(iVar18 + 1) & 1) != 0)) {
          bVar3 = true;
        }
        else {
          bVar3 = false;
        }
        if ((bVar3) &&
           ((((*(byte *)(iVar18 + -0xd) & 1) == 0 ||
             (((sVar2 = *(short *)(iVar18 + -8), sVar2 != 5 && (sVar2 != 0xec)) && (sVar2 != 0xee)))
             ) && (iVar14 = FUN_00752258(iVar7,param_2), iVar14 != 0)))) {
          *(undefined2 *)(iVar18 + 6) = 0xd3;
          FUN_007729e8(iVar7,param_2,0xffffffff);
          FUN_00649a94(iVar7,param_2,0,0);
          FUN_007ab078(auStack_98,"Created CHLOROPHYTE_ORE");
          FUN_007a7b00(auStack_98,0x37,200,0x37,600);
          iVar5 = 0x50;
          goto LAB_007823f0;
        }
      }
    }
    else if (uVar1 == 0x75) goto LAB_00782236;
  }
  else if ((uVar1 == 0xd3) && (DAT_01033960 + DAT_00902ebc >> 1 < param_2)) {
    iVar7 = FUN_00608330(&DAT_00fd67f4,4);
    if (iVar7 == 0) {
      param_1 = local_e8 + 1;
    }
    else if (iVar7 == 1) {
      param_1 = local_e8 + -1;
    }
    else if (iVar7 == 2) {
      param_2 = param_2 + 1;
    }
    else {
      param_2 = param_2 + -1;
    }
    iVar7 = (iVar17 * param_1 + param_2) * 0xe + iVar5;
    if (((*(byte *)(iVar7 + 1) & 1) != 0) &&
       (((*(short *)(iVar7 + 6) == 0x3b || (*(short *)(iVar7 + 6) == 0x3c)) &&
        (iVar18 = FUN_00752258(param_1,param_2), iVar18 != 0)))) {
      *(undefined2 *)(iVar7 + 6) = 0xd3;
      FUN_007729e8(param_1,param_2,0xffffffff);
      FUN_00649a94(param_1,param_2,0,0);
      FUN_007ab078(auStack_c8,"Grown CHLOROPHYTE_ORE");
      FUN_007a7b00(auStack_c8,0x37,200,0x37,600);
      iVar5 = 0x20;
LAB_007823f0:
      FUN_007aa754((int)&local_e8 + iVar5);
      iVar17 = DAT_00902934;
      iVar5 = DAT_00902928;
    }
  }
LAB_00782402:
  fVar4 = DAT_00782460;
  uVar10 = DAT_00fd67f8;
  uVar16 = DAT_00fd6800;
  uVar8 = DAT_00fd67fc;
  if (DAT_00fe1f8d != '\0') {
    uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    uVar16 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar20 = (float)VectorSignedToFloat(uVar16 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67f4 = DAT_00fd67f8;
    uVar10 = DAT_00fd67fc;
    uVar8 = DAT_00fd6800;
    if ((int)(fVar20 * DAT_00782460 * 3.0) != 0) {
      DAT_00fd67f8 = DAT_00fd67fc;
      DAT_00fd67fc = DAT_00fd6800;
      DAT_00fd6800 = uVar16;
      return;
    }
  }
  DAT_00fd67fc = uVar8;
  DAT_00fd6800 = uVar16;
  DAT_00fd67f8 = uVar10;
  uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
  uVar10 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
  fVar20 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  if ((int)(fVar20 * DAT_00782460 * 4.0) == 0) {
switchD_007824da_caseD_72:
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = uVar10;
    return;
  }
  uVar1 = *(ushort *)(local_e0 + 6);
  if (0x99 < uVar1) {
    if (uVar1 < 0xcc) {
      if (uVar1 != 0xcb) {
        if (uVar1 < 0xa5) {
          if (uVar1 == 0xa4) goto switchD_007824da_caseD_71;
          if (uVar1 != 0xa3) {
            DAT_00fd67f4 = DAT_00fd67f8;
            DAT_00fd67f8 = DAT_00fd67fc;
            DAT_00fd67fc = DAT_00fd6800;
            DAT_00fd6800 = uVar10;
            return;
          }
          goto switchD_007824da_caseD_70;
        }
        if (uVar1 < 199) {
          DAT_00fd67f4 = DAT_00fd67f8;
          DAT_00fd67f8 = DAT_00fd67fc;
          DAT_00fd67fc = DAT_00fd6800;
          DAT_00fd6800 = uVar10;
          return;
        }
        if (0xc9 < uVar1) {
          DAT_00fd67f4 = DAT_00fd67f8;
          DAT_00fd67f8 = DAT_00fd67fc;
          DAT_00fd67fc = DAT_00fd6800;
          DAT_00fd6800 = uVar10;
          return;
        }
      }
    }
    else if ((uVar1 != 0xcd) && (uVar1 != 0xea)) {
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      DAT_00fd67fc = DAT_00fd6800;
      DAT_00fd6800 = uVar10;
      return;
    }
    do {
      uVar16 = DAT_00fd67f8 ^ DAT_00fd67f8 << 0xb;
      uVar16 = uVar16 ^ (uVar16 ^ uVar10 >> 0xb) >> 8 ^ uVar10;
      uVar8 = DAT_00fd67fc ^ DAT_00fd67fc << 0xb;
      fVar20 = (float)VectorSignedToFloat(uVar16 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      iVar18 = (local_e8 - (int)(fVar20 * fVar4 * -7.0)) + -3;
      uVar8 = uVar8 ^ (uVar8 ^ uVar16 >> 0xb) >> 8 ^ uVar16;
      fVar20 = (float)VectorSignedToFloat(uVar8 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      iVar7 = (local_d8 - (int)(fVar20 * fVar4 * -7.0)) + -3;
      iVar5 = (iVar17 * iVar18 + iVar7) * 0xe + iVar5;
      if ((*(short *)(iVar5 + -8) == 0x1b) && ((*(byte *)(iVar5 + -0xd) & 1) != 0)) {
        bVar3 = true;
      }
      else {
        bVar3 = false;
      }
      if (bVar3) {
        DAT_00fd67f4 = DAT_00fd6800;
        DAT_00fd67f8 = uVar10;
        DAT_00fd67fc = uVar16;
        DAT_00fd6800 = uVar8;
        return;
      }
      uVar11 = (uint)*(ushort *)(iVar5 + 6);
      DAT_00fd67f4 = DAT_00fd6800;
      DAT_00fd67f8 = uVar10;
      DAT_00fd67fc = uVar16;
      if (((&DAT_010262d0)[uVar11 * 4] & 0x200000) == 0) {
        if (0x3b < uVar11) {
          if (uVar11 == 0x3c) {
LAB_00782948:
            uVar12 = 199;
          }
          else {
            if (uVar11 == 0x45) {
              DAT_00fd6800 = uVar8;
              FUN_00761b80(iVar18,iVar7);
              goto LAB_0078294c;
            }
            if (uVar11 != 0xa1) {
              DAT_00fd6800 = uVar8;
              return;
            }
            uVar12 = 200;
          }
          goto LAB_0078294a;
        }
        if (uVar11 != 0x3b) {
          if (uVar11 != 1) {
            if (uVar11 != 2) {
              if (uVar11 != 0x35) {
                DAT_00fd6800 = uVar8;
                return;
              }
              uVar12 = 0xea;
              goto LAB_0078294a;
            }
            goto LAB_00782948;
          }
          goto LAB_0078290e;
        }
        DAT_00fd6800 = uVar8;
        *(undefined2 *)(iVar5 + 6) = 0;
      }
      else {
LAB_0078290e:
        uVar12 = 0xcb;
LAB_0078294a:
        DAT_00fd6800 = uVar8;
        *(undefined2 *)(iVar5 + 6) = uVar12;
      }
LAB_0078294c:
      FUN_007729e8(iVar18,iVar7,0xffffffff);
      FUN_00649a94(iVar18,iVar7,0,0);
      FUN_007ab078(auStack_68,"Spread Crimson");
      FUN_007a7b00(auStack_68,0x37,200,0x37,600);
      FUN_007aa754(auStack_68);
      uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      uVar10 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      fVar20 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      iVar17 = DAT_00902934;
      iVar5 = DAT_00902928;
      if ((int)(fVar20 * fVar4 * 2.0) != 0) {
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        DAT_00fd67fc = DAT_00fd6800;
        DAT_00fd6800 = uVar10;
        return;
      }
    } while( true );
  }
  if (uVar1 != 0x99) {
    if (0x6e < uVar1) {
      switch(uVar1) {
      case 0x70:
        goto switchD_007824da_caseD_70;
      case 0x71:
      case 0x73:
      case 0x74:
      case 0x75:
      case 0x76:
        goto switchD_007824da_caseD_71;
      default:
        goto switchD_007824da_caseD_72;
      }
    }
    if (0x6c < uVar1) {
switchD_007824da_caseD_71:
      uVar16 = DAT_0103089c;
      uVar8 = DAT_010308a4;
      DAT_010308a4 = DAT_010308a0;
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      DAT_00fd67fc = DAT_00fd6800;
      DAT_00fd6800 = uVar10;
      do {
        uVar10 = DAT_01030898 ^ DAT_01030898 << 0xb;
        iVar7 = 10;
        DAT_0103089c = uVar8;
        uVar10 = uVar10 ^ (uVar10 ^ uVar8 >> 0xb) >> 8 ^ uVar8;
        DAT_01030898 = DAT_010308a4;
        while( true ) {
          DAT_010308a0 = uVar10;
          uVar16 = uVar16 ^ uVar16 << 0xb;
          fVar20 = (float)VectorSignedToFloat(DAT_010308a0 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3
                                             );
          iVar7 = iVar7 + -1;
          local_e0 = (local_e8 - (int)(fVar20 * fVar4 * -7.0)) + -3;
          local_dc = uVar16 ^ (uVar16 ^ DAT_010308a0 >> 0xb) >> 8 ^ DAT_010308a0;
          fVar20 = (float)VectorSignedToFloat(local_dc & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          iVar14 = (local_d8 - (int)(fVar20 * fVar4 * -7.0)) + -3;
          iVar18 = (iVar17 * local_e0 + iVar14) * 0xe + iVar5;
          if ((iVar7 < 1) && ((*(byte *)(iVar18 + 1) & 1) != 0)) break;
          uVar10 = DAT_01030898 ^ DAT_01030898 << 0xb;
          uVar16 = DAT_0103089c;
          DAT_0103089c = local_dc;
          uVar10 = uVar10 ^ (uVar10 ^ local_dc >> 0xb) >> 8 ^ local_dc;
          DAT_01030898 = DAT_010308a0;
        }
        uVar10 = (uint)*(ushort *)(iVar18 + 6);
        DAT_010308a4 = local_dc;
        if (((&DAT_010262d0)[uVar10 * 4] & 0x200000) == 0) {
          if (0x3b < uVar10) {
            if ((uVar10 != 0x7f) && (uVar10 != 0xa1)) {
              return;
            }
            uVar12 = 0xa4;
            goto LAB_007827a8;
          }
          if (uVar10 != 0x3b) {
            if (uVar10 == 1) goto LAB_00782776;
            if (uVar10 == 2) {
              uVar12 = 0x6d;
            }
            else {
              if (uVar10 != 0x35) {
                return;
              }
              uVar12 = 0x74;
            }
            goto LAB_007827a8;
          }
          *(undefined2 *)(iVar18 + 6) = 0;
        }
        else {
LAB_00782776:
          uVar12 = 0x75;
LAB_007827a8:
          *(undefined2 *)(iVar18 + 6) = uVar12;
        }
        local_e4 = iVar14;
        FUN_007729e8(local_e0,iVar14,0xffffffff);
        FUN_00649a94(local_e0,iVar14,0,0);
        FUN_007ab078(auStack_98,"Spread Hallow");
        FUN_007a7b00(auStack_98,0x37,200,0x37,600);
        FUN_007aa754(auStack_98);
        uVar10 = DAT_01030898 ^ DAT_01030898 << 0xb;
        uVar8 = uVar10 ^ (uVar10 ^ DAT_010308a4 >> 0xb) >> 8 ^ DAT_010308a4;
        fVar20 = (float)VectorSignedToFloat(uVar8 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        uVar16 = DAT_010308a0;
        DAT_01030898 = DAT_0103089c;
        iVar17 = DAT_00902934;
        iVar5 = DAT_00902928;
        if ((int)(fVar20 * fVar4 * 2.0) == 0) {
          DAT_0103089c = DAT_010308a0;
          DAT_010308a0 = DAT_010308a4;
          DAT_010308a4 = uVar8;
          return;
        }
      } while( true );
    }
    if (((uVar1 != 0x17) && (uVar1 != 0x19)) && (uVar1 != 0x20)) {
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      DAT_00fd67fc = DAT_00fd6800;
      DAT_00fd6800 = uVar10;
      return;
    }
  }
switchD_007824da_caseD_70:
  do {
    uVar16 = DAT_00fd67f8 ^ DAT_00fd67f8 << 0xb;
    uVar16 = uVar16 ^ (uVar16 ^ uVar10 >> 0xb) >> 8 ^ uVar10;
    fVar20 = (float)VectorSignedToFloat(uVar16 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    iVar7 = (local_e8 - (int)(fVar20 * fVar4 * -7.0)) + -3;
    uVar8 = DAT_00fd67fc ^ DAT_00fd67fc << 0xb;
    uVar8 = uVar8 ^ (uVar8 ^ uVar16 >> 0xb) >> 8 ^ uVar16;
    fVar20 = (float)VectorSignedToFloat(uVar8 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    iVar18 = (local_d8 - (int)(fVar20 * fVar4 * -7.0)) + -3;
    iVar5 = (iVar17 * iVar7 + iVar18) * 0xe + iVar5;
    if ((*(short *)(iVar5 + -8) == 0x1b) && ((*(byte *)(iVar5 + -0xd) & 1) != 0)) {
      bVar3 = true;
    }
    else {
      bVar3 = false;
    }
    if (bVar3) {
      DAT_00fd67f4 = DAT_00fd6800;
      DAT_00fd67f8 = uVar10;
      DAT_00fd67fc = uVar16;
      DAT_00fd6800 = uVar8;
      return;
    }
    uVar11 = (uint)*(ushort *)(iVar5 + 6);
    DAT_00fd67f4 = DAT_00fd6800;
    DAT_00fd67f8 = uVar10;
    DAT_00fd67fc = uVar16;
    if (((&DAT_010262d0)[uVar11 * 4] & 0x200000) == 0) {
      if (0x3b < uVar11) {
        if (uVar11 == 0x3c) {
LAB_007825f8:
          uVar12 = 0x17;
        }
        else if (uVar11 == 0x45) {
          uVar12 = 0x20;
        }
        else {
          if (uVar11 != 0xa1) {
            DAT_00fd6800 = uVar8;
            return;
          }
          uVar12 = 0xa3;
        }
        goto LAB_007825fa;
      }
      if (uVar11 != 0x3b) {
        if (uVar11 == 1) goto LAB_007825c0;
        if (uVar11 == 2) goto LAB_007825f8;
        if (uVar11 != 0x35) {
          DAT_00fd6800 = uVar8;
          return;
        }
        uVar12 = 0x70;
        goto LAB_007825fa;
      }
      DAT_00fd6800 = uVar8;
      *(undefined2 *)(iVar5 + 6) = 0;
    }
    else {
LAB_007825c0:
      uVar12 = 0x19;
LAB_007825fa:
      DAT_00fd6800 = uVar8;
      *(undefined2 *)(iVar5 + 6) = uVar12;
    }
    local_e4 = iVar7;
    FUN_007729e8(iVar7,iVar18,0xffffffff);
    FUN_00649a94(iVar7,iVar18,0,0);
    FUN_007ab078(auStack_c8,"Spread Corruption");
    FUN_007a7b00(auStack_c8,0x37,200,0x37,600);
    FUN_007aa754(auStack_c8);
    uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    uVar10 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar20 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    iVar17 = DAT_00902934;
    iVar5 = DAT_00902928;
    if ((int)(fVar20 * fVar4 * 2.0) != 0) {
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      DAT_00fd67fc = DAT_00fd6800;
      DAT_00fd6800 = uVar10;
      return;
    }
  } while( true );
}

