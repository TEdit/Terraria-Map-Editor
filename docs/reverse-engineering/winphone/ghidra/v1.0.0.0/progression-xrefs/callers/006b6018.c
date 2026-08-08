
void FUN_006b6018(void)

{
  uint uVar1;
  bool bVar2;
  byte bVar3;
  double dVar4;
  bool bVar5;
  double dVar6;
  float fVar7;
  double dVar8;
  double dVar9;
  float fVar10;
  undefined1 uVar11;
  int iVar12;
  int iVar13;
  int iVar14;
  uint uVar15;
  int iVar16;
  int *piVar17;
  int *piVar18;
  int iVar19;
  undefined4 uVar20;
  uint uVar21;
  undefined4 uVar22;
  int iVar23;
  uint in_fpscr;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  double dVar28;
  double dVar29;
  float fVar30;
  float fVar31;
  undefined1 local_170;
  byte local_16f;
  char local_16e [2];
  int local_16c;
  int local_168;
  int local_164;
  uint local_160;
  int local_15c;
  int local_158;
  uint local_154;
  int local_150;
  int local_14c;
  uint local_148;
  int local_144;
  int local_140;
  uint local_13c;
  int local_138;
  int local_134;
  float local_130;
  int local_12c;
  int local_128 [2];
  int local_120;
  int local_11c;
  int local_118;
  int local_114;
  undefined1 local_110;
  undefined1 local_10f;
  undefined1 local_10e;
  int local_10c;
  int local_108;
  float local_f0;
  float local_ec;
  float local_e8;
  float local_e4;
  float local_e0;
  float local_dc;
  int local_d8;
  float local_d4;
  int local_d0;
  float local_cc;
  float local_c8;
  int local_c4;
  float local_c0;
  int local_bc;
  float local_b8;
  int local_b4;
  float local_b0;
  int local_ac;
  double local_a8;
  double local_a0;
  double local_98;
  double local_90;
  double local_88;
  double local_80;
  double local_78;
  undefined4 local_70;
  undefined4 uStack_6c;
  
  fVar10 = DAT_006b6344;
  dVar9 = DAT_006b633c;
  dVar8 = DAT_006b6330;
  fVar7 = DAT_006b632c;
  fVar26 = DAT_006b6328;
  dVar6 = DAT_006b6318;
  local_70 = 0xfffffffe;
  uStack_6c = 0xfffffffe;
  if (DAT_00fe1f72 != '\0') {
    DAT_00fe1f72 = 0;
    return;
  }
  local_16e[0] = '\0';
  local_164 = 0x1f5;
  local_158 = 0;
  local_128[0] = 0;
  local_16c = 0;
  local_12c = 0;
  bVar2 = *(char *)(DAT_0107034c + 0x5cc5) != '\0';
  local_154 = (uint)bVar2;
  if (*(char *)(DAT_01070350 + 0x5cc5) != '\0') {
    local_154 = bVar2 + 1;
  }
  if (*(char *)(DAT_01070354 + 0x5cc5) != '\0') {
    local_154 = local_154 + 1;
  }
  if (*(char *)(DAT_01070358 + 0x5cc5) != '\0') {
    local_154 = local_154 + 1;
  }
  local_e8 = 6.0;
  local_a8 = DAT_006b6358;
  local_80 = 0.75;
  local_f0 = DAT_006b634c;
  local_c8 = 0.5;
  local_cc = DAT_006b6348;
  local_e0 = DAT_006b6338;
  local_15c = DAT_00902928;
  local_140 = 0;
  local_98 = DAT_006b6320;
  local_c0 = DAT_006b6314;
  local_78 = DAT_006b6350;
  local_b0 = DAT_006b6310;
  local_ec = DAT_006b630c;
  local_e4 = DAT_006b6308;
  local_dc = 1.5;
  local_88 = DAT_006b6300;
  local_d4 = 3.0;
  dVar29 = (double)VectorSignedToFloat(local_154,(byte)(in_fpscr >> 0x16) & 3);
  local_130 = DAT_006b62fc;
  fVar24 = (float)VectorSignedToFloat(local_154,(byte)(in_fpscr >> 0x16) & 3);
  dVar28 = (double)VectorSignedToFloat(local_154,(byte)(in_fpscr >> 0x16) & 3);
  local_b8 = 1.125 - fVar24 * 0.125;
  local_a0 = dVar28 * DAT_006b6350;
  local_90 = dVar29 * DAT_006b6350;
  uVar15 = local_154;
  do {
    iVar12 = (&DAT_0107034c)[local_140];
    if ((*(char *)(iVar12 + 0x5cc5) != '\0') && (*(char *)(iVar12 + 0x5c18) == '\0')) {
      local_160 = 0;
      local_138 = 0;
      local_144 = 0;
      local_150 = 0;
      if ((0 < DAT_010339a0) &&
         (((DAT_010339ac == 0 && (0 < DAT_010339a8)) &&
          (*(int *)(iVar12 + 0x104) < DAT_01050304 + 0x438)))) {
        fVar27 = (float)VectorSignedToFloat(*(undefined4 *)(iVar12 + 0x100),
                                            (byte)(in_fpscr >> 0x16) & 3);
        fVar24 = DAT_010339a4 * 16.0 - fVar10;
        uVar21 = in_fpscr & 0xfffffff;
        uVar1 = uVar21 | (uint)(fVar27 < fVar24) << 0x1f | (uint)(fVar27 == fVar24) << 0x1e;
        in_fpscr = uVar1 | (uint)(NAN(fVar27) || NAN(fVar24)) << 0x1c;
        bVar3 = (byte)(uVar1 >> 0x18);
        if ((!(bool)(bVar3 >> 6 & 1) && bVar3 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) &&
           (in_fpscr = uVar21 | (uint)(DAT_010339a4 * 16.0 + fVar10 <= fVar27) << 0x1d,
           (byte)(in_fpscr >> 0x1d) == 0)) {
          local_150 = 1;
        }
      }
      local_13c = 0;
      local_16f = 0;
      DAT_00902534 = 800;
      DAT_00902538 = 4;
      iVar13 = 800;
      if ((DAT_010703e6 < 3) && (iVar13 = 800, DAT_010338d3 == '\0')) {
        iVar12 = FUN_006c1308(4,iVar12,800);
        if (*(short *)(iVar12 + 0x5cfa) < 2) {
          iVar12 = 3 - DAT_010703e6;
          DAT_00902538 = DAT_00902538 - iVar12;
          iVar13 = (int)((ulonglong)((longlong)iVar12 * (longlong)DAT_006b62f8) >> 0x20);
          fVar24 = (float)VectorSignedToFloat(iVar13 - (iVar13 >> 0x1f),(byte)(in_fpscr >> 0x16) & 3
                                             );
          local_15c = DAT_00902928;
          fVar27 = (float)VectorSignedToFloat(DAT_00902534,(byte)(in_fpscr >> 0x16) & 3);
          iVar13 = (int)((fVar24 + 1.0) * fVar27);
          uVar15 = local_154;
        }
        else {
          local_15c = DAT_00902928;
          iVar12 = DAT_00902928;
          iVar13 = DAT_00902534;
        }
      }
      if (DAT_010338d3 == '\x01') {
        DAT_00902534 = 800;
        iVar12 = FUN_006c1308(DAT_00902538,iVar12,iVar13);
        iVar13 = DAT_00902534;
        if (0x12 < *(short *)(iVar12 + 0x5cfa)) {
          iVar13 = 0x2a8;
        }
        iVar12 = 3 - DAT_010703e8;
        DAT_00902538 = DAT_00902538 + 1;
        local_15c = DAT_00902928;
        if (0 < iVar12) {
          fVar24 = (float)VectorSignedToFloat(iVar12,(byte)(in_fpscr >> 0x16) & 3);
          fVar27 = (float)VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00902538 = DAT_00902538 - iVar12;
          iVar13 = (int)((fVar24 / local_e8 + 1.0) * fVar27);
        }
      }
      iVar12 = DAT_00902538 + uVar15;
      local_b4 = iVar12 + -1;
      fVar24 = (float)VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
      iVar13 = (int)(fVar24 * local_b8);
      dVar29 = (double)VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
      dVar28 = local_a8;
      if (DAT_010338d3 != '\0') {
        dVar28 = dVar9;
      }
      dVar4 = dVar9;
      if (DAT_010338d3 != '\0') {
        dVar4 = dVar8;
      }
      fVar24 = (float)VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
      local_c4 = (int)(fVar24 * fVar7);
      local_168 = (int)(longlong)(dVar29 * dVar9);
      local_bc = (int)(longlong)(dVar29 * local_a8);
      local_d8 = (int)(fVar24 * fVar26);
      local_148 = local_bc;
      if (DAT_010338d3 != '\0') {
        local_148 = local_168;
      }
      fVar30 = (float)VectorSignedToFloat(local_b4,(byte)(in_fpscr >> 0x16) & 3);
      iVar16 = (&DAT_0107034c)[local_140];
      fVar31 = (float)VectorSignedToFloat((DAT_00902ea4 + -200) * 0x10,(byte)(in_fpscr >> 0x16) & 3)
      ;
      fVar27 = *(float *)(iVar16 + 0x114);
      local_134 = iVar16;
      local_14c = iVar13 * 3;
      iVar14 = iVar12 + 1;
      uVar15 = in_fpscr & 0xfffffff;
      uVar21 = uVar15 | (uint)(fVar27 < fVar31) << 0x1f | (uint)(fVar27 == fVar31) << 0x1e;
      local_d0 = local_b4;
      local_ac = local_b4 / 2;
      bVar3 = (byte)(uVar21 >> 0x18);
      DAT_00902534 = iVar13;
      DAT_00902538 = iVar14;
      if ((bool)(bVar3 >> 6 & 1) || (bool)(bVar3 >> 7) != (NAN(fVar27) || NAN(fVar31))) {
        fVar25 = (float)VectorSignedToFloat(DAT_01033960 * 0x10 + 0x438,(byte)(uVar21 >> 0x16) & 3);
        uVar21 = uVar15 | (uint)(fVar27 < fVar25) << 0x1f | (uint)(fVar27 == fVar25) << 0x1e;
        bVar3 = (byte)(uVar21 >> 0x18);
        DAT_00902534 = (int)(longlong)(dVar29 * dVar28);
        if ((bool)(bVar3 >> 6 & 1) || (bool)(bVar3 >> 7) != (NAN(fVar27) || NAN(fVar25))) {
          fVar25 = (float)VectorSignedToFloat(DAT_00902ebc * 0x10 + 0x438,(byte)(uVar21 >> 0x16) & 3
                                             );
          uVar21 = uVar15 | (uint)(fVar27 < fVar25) << 0x1f | (uint)(fVar27 == fVar25) << 0x1e;
          bVar3 = (byte)(uVar21 >> 0x18);
          DAT_00902534 = (int)(longlong)(dVar29 * dVar4);
          DAT_00902538 = iVar12;
          if ((bool)(bVar3 >> 6 & 1) || (bool)(bVar3 >> 7) != (NAN(fVar27) || NAN(fVar25))) {
            DAT_00902538 = local_b4;
            if (DAT_010703e0 == '\0') {
              DAT_00902534 = (int)(longlong)(dVar29 * local_80);
              if (DAT_010703e2 != '\0') {
                DAT_00902534 = local_168;
                DAT_00902538 = iVar14;
              }
            }
            else {
              DAT_00902534 = iVar13;
              if (DAT_010703e1 != '\0') {
                DAT_00902534 = (int)(fVar24 * local_f0);
                DAT_00902538 = (int)(fVar30 * local_cc);
              }
            }
          }
        }
      }
      piVar18 = &DAT_00902538;
      if (*(char *)(iVar16 + 0xea) != '\0') {
        fVar27 = (float)VectorSignedToFloat(DAT_00902ebc,(byte)(uVar21 >> 0x16) & 3);
        uVar21 = uVar21 & 0xfffffff;
        if (*(float *)(iVar16 + 0x114) * local_e0 < fVar27) {
          DAT_00902534 = (int)(((1.0 - DAT_010299b0) + 1.0) * fVar24 * local_c8);
          DAT_00902538 = (int)(fVar30 + fVar30 * DAT_010299b0);
        }
      }
      iVar13 = local_c4;
      if (((*(char *)(iVar16 + 0xe5) == '\0') &&
          (iVar13 = (int)(fVar24 * fVar26), iVar14 = iVar12, *(char *)(iVar16 + 0xe9) == '\0')) &&
         ((iVar19 = local_148, *(char *)(iVar16 + 0xe6) != '\0' ||
          ((*(char *)(iVar16 + 0xeb) != '\0' ||
           (iVar19 = local_bc, iVar13 = DAT_00902534, iVar14 = DAT_00902538,
           *(char *)(iVar16 + 0xe8) != '\0')))))) {
        DAT_00902534 = iVar19;
        iVar13 = DAT_00902534;
        iVar14 = local_b4;
      }
      DAT_00902538 = iVar14;
      DAT_00902534 = iVar13;
      if ((*(char *)(iVar16 + 0xe7) != '\0') &&
         (DAT_01033960 * 0x10 + 0x438 < *(int *)(iVar16 + 0x104))) {
        DAT_00902534 = local_168;
        DAT_00902538 = iVar12;
      }
      if (-1 < DAT_0090211c) {
        fVar24 = *(float *)(iVar16 + 0x114);
        uVar21 = uVar21 & 0xfffffff | (uint)(fVar24 < fVar31) << 0x1f |
                 (uint)(fVar24 == fVar31) << 0x1e;
        bVar3 = (byte)(uVar21 >> 0x18);
        if (!(bool)(bVar3 >> 6 & 1) && (bool)(bVar3 >> 7) == (NAN(fVar24) || NAN(fVar31))) {
          DAT_00902534 = local_14c;
          DAT_00902538 = local_b4 / 2;
        }
      }
      dVar28 = (double)VectorSignedToFloat(DAT_00902538,(byte)(uVar21 >> 0x16) & 3);
      iVar12 = (int)*(float *)(iVar16 + 0x150);
      if (iVar12 < (int)(longlong)(dVar28 * local_98)) {
        fVar24 = (float)VectorSignedToFloat(DAT_00902534,(byte)(uVar21 >> 0x16) & 3);
        fVar24 = fVar24 * local_c0;
LAB_006b67b2:
        DAT_00902534 = (int)fVar24;
      }
      else {
        if (iVar12 < (int)(longlong)(dVar28 * local_78)) {
          fVar24 = (float)VectorSignedToFloat(DAT_00902534,(byte)(uVar21 >> 0x16) & 3);
          fVar24 = fVar24 * fVar7;
          goto LAB_006b67b2;
        }
        if (iVar12 < (int)(longlong)(dVar28 * dVar6)) {
          fVar24 = (float)VectorSignedToFloat(DAT_00902534,(byte)(uVar21 >> 0x16) & 3);
          fVar24 = fVar24 * fVar26;
          goto LAB_006b67b2;
        }
      }
      if ((*(char *)(iVar16 + 0xe6) == '\0') && (*(char *)(iVar16 + 0xeb) == '\0')) {
        bVar2 = false;
      }
      else {
        bVar2 = true;
      }
      fVar27 = *(float *)(iVar16 + 0x114) * 16.0;
      fVar24 = (float)VectorSignedToFloat(DAT_01033960 + DAT_00902ebc >> 1,
                                          (byte)(uVar21 >> 0x16) & 3);
      uVar21 = uVar21 & 0xfffffff;
      uVar15 = uVar21 | (uint)(fVar27 < fVar24) << 0x1f | (uint)(fVar27 == fVar24) << 0x1e;
      in_fpscr = uVar15 | (uint)(NAN(fVar27) || NAN(fVar24)) << 0x1c;
      bVar3 = (byte)(uVar15 >> 0x18);
      if ((!(bool)(bVar3 >> 6 & 1) && bVar3 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) || (bVar2)) {
        dVar29 = (double)VectorSignedToFloat(iVar12,(byte)(in_fpscr >> 0x16) & 3);
        in_fpscr = uVar21 | (uint)(dVar28 * local_98 <= dVar29) << 0x1d;
        fVar24 = local_b0;
        if (((byte)(in_fpscr >> 0x1d) == 0) ||
           (in_fpscr = uVar21 | (uint)(dVar28 * dVar6 <= dVar29) << 0x1d, fVar24 = local_ec,
           (byte)(in_fpscr >> 0x1d) == 0)) {
          fVar27 = (float)VectorSignedToFloat(DAT_00902534,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00902534 = (int)(fVar27 * fVar24);
        }
      }
      if ((*(int *)(iVar16 + *(char *)(iVar16 + 0x14c) * 0xa0 + 0x94c) == 0x94) ||
         (*(char *)(iVar16 + 0xec) != '\0')) {
        fVar24 = (float)VectorSignedToFloat(DAT_00902534,(byte)(in_fpscr >> 0x16) & 3);
        fVar27 = (float)VectorSignedToFloat(DAT_00902538,(byte)(in_fpscr >> 0x16) & 3);
        DAT_00902534 = (int)(fVar24 * local_e4);
        DAT_00902538 = (int)(fVar27 * local_dc);
      }
      if (*(char *)(iVar16 + 0x8dd7) == '\x01') {
        dVar28 = (double)VectorSignedToFloat(DAT_00902534,(byte)(in_fpscr >> 0x16) & 3);
        fVar24 = (float)VectorSignedToFloat(DAT_00902538,(byte)(in_fpscr >> 0x16) & 3);
        DAT_00902534 = (int)(longlong)(dVar28 * local_88);
        DAT_00902538 = (int)(fVar24 * 2.0);
      }
      if (DAT_00902534 < 0x50) {
        DAT_00902534 = 0x50;
      }
      if (0xc < DAT_00902538) {
        DAT_00902538 = 0xc;
      }
      if (DAT_010703e3 != '\0') {
        local_14c = 10;
        piVar17 = piVar18;
        if (9 < (int)(longlong)((local_a0 + 1.0) * 4.0)) {
          piVar17 = &local_14c;
        }
        DAT_00902538 = *piVar17;
        DAT_00902534 = 100;
      }
      if (local_150 != 0) {
        local_14c = 10;
        if (9 < (int)(longlong)((local_90 + 1.0) * 4.0)) {
          piVar18 = &local_14c;
        }
        DAT_00902538 = *piVar18;
        DAT_00902534 = 100;
      }
      iVar12 = DAT_00902538;
      if ((*(char *)(iVar16 + 0xe5) != '\0') && (DAT_00fe1f7b == '\0')) {
        DAT_00902534 = 10;
      }
      iVar13 = DAT_00902534;
      bVar2 = false;
      local_170 = 0;
      if (((local_150 == 0) &&
          (((((DAT_010703e2 == '\0' && (DAT_010703e3 == '\0')) || (DAT_010703e0 != '\0')) &&
            ((DAT_010703e1 == '\0' || (DAT_010703e0 == '\0')))) &&
           (*(char *)(iVar16 + 0xe5) == '\0')))) &&
         (((*(char *)(iVar16 + 0xe6) == '\0' && (*(char *)(iVar16 + 0xeb) == '\0')) &&
          (*(char *)(iVar16 + 0xe8) == '\0')))) {
        fVar24 = *(float *)(iVar16 + 0xf4);
        uVar15 = in_fpscr & 0xfffffff;
        in_fpscr = uVar15 | (uint)(fVar24 == 1.0) << 0x1e;
        if ((byte)(in_fpscr >> 0x1e) == 0) {
          in_fpscr = uVar15 | (uint)(fVar24 == 2.0) << 0x1e;
          if ((byte)(in_fpscr >> 0x1e) == 0) {
            uVar15 = uVar15 | (uint)(fVar24 < local_d4) << 0x1f;
            in_fpscr = uVar15 | (uint)(NAN(fVar24) || NAN(local_d4)) << 0x1c;
            if ((byte)(uVar15 >> 0x1f) == ((byte)(in_fpscr >> 0x1c) & 1)) {
              fVar24 = (float)VectorSignedToFloat(DAT_00902538,(byte)(in_fpscr >> 0x16) & 3);
              local_160 = 1;
              DAT_00902538 = (int)(fVar24 * fVar26);
              goto LAB_006b6a94;
            }
          }
          else {
            local_160 = 1;
            iVar14 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar14 < 2) {
              dVar28 = (double)VectorSignedToFloat(iVar12,(byte)(in_fpscr >> 0x16) & 3);
              DAT_00902538 = (int)(longlong)(dVar28 * dVar8);
LAB_006b6a94:
              bVar2 = true;
              local_170 = 1;
              iVar12 = DAT_00902538;
            }
            else {
              iVar13 = iVar13 * 3;
              DAT_00902534 = iVar13;
            }
          }
        }
        else {
          local_160 = 1;
          iVar14 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar14 == 0) {
            dVar28 = (double)VectorSignedToFloat(iVar12,(byte)(in_fpscr >> 0x16) & 3);
            DAT_00902538 = (int)(longlong)(dVar28 * dVar8);
            goto LAB_006b6a94;
          }
          iVar13 = iVar13 << 1;
          DAT_00902534 = iVar13;
        }
      }
      uVar15 = (uint)*(byte *)((DAT_00902934 *
                                (*(int *)(iVar16 + 0x100) + (*(int *)(iVar16 + 0x108) >> 1) >> 4) +
                               (*(int *)(iVar16 + 0x104) + (*(int *)(iVar16 + 0x10c) >> 1) >> 4)) *
                               0xe + local_15c + 8);
      local_148 = (byte)(&DAT_01026168)[uVar15] | local_160;
      local_160 = (uint)(uVar15 == 0x57);
      if ((int)*(float *)(iVar16 + 0x150) < iVar12) {
        fVar27 = (float)VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
        uVar15 = DAT_01070468 ^ DAT_01070468 << 0xb;
        DAT_01070468 = DAT_0107046c;
        DAT_0107046c = DAT_01070470;
        DAT_01070470 = DAT_01070474;
        DAT_01070474 = DAT_01070474 ^ uVar15 ^ (uVar15 ^ DAT_01070474 >> 0xb) >> 8;
        fVar24 = (float)VectorSignedToFloat(DAT_01070474 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        if ((int)(fVar24 * local_130 * fVar27) == 0) {
          FUN_0065b508(*(int *)(iVar16 + 0x100) >> 4,*(int *)(iVar16 + 0x104) >> 4,local_150,
                       local_16e,&local_170,&local_164,local_128,&local_12c,&local_16f);
          local_13c = (uint)local_16f;
          local_158 = local_128[0];
          local_16c = local_12c;
        }
      }
      iVar12 = local_158;
      iVar13 = local_144;
      uVar21 = local_13c;
      if (local_13c != 0) {
        iVar14 = 0;
        piVar18 = &DAT_0107034c;
        do {
          iVar16 = *piVar18;
          if (*(char *)(iVar16 + 0x5cc5) != '\0') {
            iVar19 = *(int *)(iVar16 + 0x104) + (uint)(DAT_00878508 >> 1);
            iVar16 = *(int *)(iVar16 + 0x100) + (uint)(DAT_00878504 >> 1);
            if ((((iVar16 + -0x3fe < local_158 * 0x10 + 0x10) && (local_158 * 0x10 < iVar16 + 0x3fe)
                 ) && (iVar19 + -0x23e < local_16c * 0x10 + 0x10)) &&
               (local_16c * 0x10 < iVar19 + 0x23e)) {
              bVar5 = true;
            }
            else {
              bVar5 = false;
            }
            if (bVar5) {
              uVar21 = 0;
              break;
            }
          }
          iVar14 = iVar14 + 1;
          piVar18 = piVar18 + 1;
        } while (iVar14 < 4);
        if (uVar21 != 0) {
          if ((*(char *)(local_134 + 0xe5) != '\0') &&
             ((((&DAT_010262d0)
                [(uint)*(ushort *)((DAT_00902934 * local_158 + local_16c) * 0xe + local_15c + 6) * 4
                ] & 0x2000) == 0 ||
              (*(char *)((DAT_00902934 * local_158 + local_16c + -1) * 0xe + local_15c + 8) == '\0')
              ))) {
            uVar21 = 0;
          }
          iVar14 = local_16c + -1;
          if (((*(char *)((DAT_00902934 * local_158 + iVar14) * 0xe + local_15c + 4) != '\0') &&
              (*(char *)((DAT_00902934 * local_158 + local_16c + -2) * 0xe + local_15c + 4) != '\0')
              ) && ((*(ushort *)((DAT_00902934 * local_158 + iVar14) * 0xe + local_15c + 2) & 0x3000
                    ) != 0x1000)) {
            if ((*(ushort *)((DAT_00902934 * local_158 + iVar14) * 0xe + local_15c + 2) & 0x3000) ==
                0x2000) {
              iVar13 = 1;
            }
            else {
              local_138 = 1;
            }
          }
        }
      }
      if (iVar13 != 0) {
        uVar21 = 0;
      }
      uVar15 = local_154;
      if (uVar21 != 0) {
        iVar16 = local_158 * 0x10 + 8;
        iVar19 = local_16c * 0x10;
        FUN_007ab078(&local_120,"Spawning NPC");
        FUN_007a7b00(&local_120,0x37,200,0x37,600);
        FUN_007aa754(&local_120);
        iVar14 = local_164;
        iVar13 = local_16c;
        local_110 = (undefined1)local_148;
        local_118 = iVar12;
        local_10e = (undefined1)local_150;
        local_108 = local_140;
        local_114 = local_16c;
        local_10f = (undefined1)local_138;
        local_10c = local_164;
        local_120 = iVar16;
        local_11c = iVar19;
        if (!bVar2) goto LAB_006b72c4;
        if (local_138 != 0) {
          iVar13 = 0xc4;
          iVar23 = 0;
          iVar12 = DAT_00fe1fa8;
          break;
        }
        if ((local_164 == 0x93) || (local_164 == 0xa1)) {
          uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          DAT_00fd67f4 = DAT_00fd67f8;
          DAT_00fd67f8 = DAT_00fd67fc;
          uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
          DAT_00fd67fc = DAT_00fd6800;
          fVar26 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          iVar12 = 0x94 - (int)(fVar26 * local_130 * -2.0);
          DAT_00fd6800 = uVar15;
        }
        else {
          if (((local_16c <= DAT_00902ebc) && (local_164 != 2)) && (local_164 != 0x6d)) {
            return;
          }
          if (DAT_010299a0 != '\0') {
            iVar13 = 0xc4;
            iVar23 = 0;
            iVar12 = DAT_00fe1fa8;
            goto LAB_006b6ea0;
          }
          uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          DAT_00fd67f4 = DAT_00fd67f8;
          DAT_00fd67f8 = DAT_00fd67fc;
          uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
          DAT_00fd67fc = DAT_00fd6800;
          fVar26 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00fd6800 = uVar15;
          if (((int)(fVar26 * local_130 * 2.0) == 0) && (local_16c <= DAT_00902ebc)) {
            iVar12 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar12 == 0) {
              iVar13 = 0xc4;
              iVar23 = 0;
              iVar12 = DAT_00fe1fa8;
              goto LAB_006b6fda;
            }
            if (iVar12 != 1) {
              iVar13 = 0xc4;
              iVar23 = 0;
              iVar12 = DAT_00fe1fa8;
              goto LAB_006b716a;
            }
            iVar13 = 0xc4;
            iVar23 = 0;
            iVar12 = DAT_00fe1fa8;
            goto LAB_006b70a4;
          }
          iVar12 = FUN_007354a8();
          if ((iVar12 == 0) || (iVar12 = FUN_00608330(&DAT_00fd67f4,3), iVar12 == 0)) {
            if ((DAT_00902ebc < iVar13) || (iVar12 = FUN_00608330(&DAT_00fd67f4,4), iVar12 != 0)) {
              FUN_006921b0(iVar16,iVar19);
              goto LAB_006b72c4;
            }
            iVar12 = 299;
          }
          else {
            iVar12 = 0x12f;
          }
        }
        iVar13 = FUN_00685550(iVar16,iVar19,iVar12,0);
        goto LAB_006b72bc;
      }
    }
    local_140 = local_140 + 1;
    if (3 < local_140) {
      return;
    }
  } while( true );
  while( true ) {
    iVar23 = iVar23 + 1;
    iVar12 = iVar12 + 0x274;
    if (0xc3 < iVar23) break;
    if (*(char *)(iVar12 + 0x100) == '\0') {
      iVar13 = iVar23;
      if (iVar23 < 0xc4) {
        FUN_00684770(iVar23 * 0x274 + DAT_00fe1fa8,0x37);
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar16 - (uint)(*(ushort *)(iVar12 + 0x168) >> 1);
        *(int *)(iVar12 + 0x148) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x138) = uVar22;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar19 - (uint)*(ushort *)(iVar12 + 0x16a);
        *(int *)(iVar12 + 0x14c) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x13c) = uVar22;
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
        *(undefined4 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        uVar11 = FUN_0060a19c(iVar12 + 0x138,*(undefined2 *)(iVar12 + 0x168),
                              *(undefined2 *)(iVar12 + 0x16a));
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 100) = uVar11;
        FUN_00649e08(iVar23,1);
        iVar14 = local_164;
      }
      break;
    }
  }
  goto LAB_006b72bc;
  while( true ) {
    iVar23 = iVar23 + 1;
    iVar12 = iVar12 + 0x274;
    if (0xc3 < iVar23) break;
LAB_006b6ea0:
    if (*(char *)(iVar12 + 0x100) == '\0') {
      iVar13 = iVar23;
      if (iVar23 < 0xc4) {
        FUN_00684770(iVar23 * 0x274 + DAT_00fe1fa8,0xe6);
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar16 - (uint)(*(ushort *)(iVar12 + 0x168) >> 1);
        *(int *)(iVar12 + 0x148) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x138) = uVar22;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar19 - (uint)*(ushort *)(iVar12 + 0x16a);
        *(int *)(iVar12 + 0x14c) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x13c) = uVar22;
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
        *(undefined4 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        uVar11 = FUN_0060a19c(iVar12 + 0x138,*(undefined2 *)(iVar12 + 0x168),
                              *(undefined2 *)(iVar12 + 0x16a));
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 100) = uVar11;
        FUN_00649e08(iVar23,1);
        iVar14 = local_164;
      }
      break;
    }
  }
  goto LAB_006b72bc;
  while( true ) {
    iVar23 = iVar23 + 1;
    iVar12 = iVar12 + 0x274;
    if (0xc3 < iVar23) break;
LAB_006b6fda:
    if (*(char *)(iVar12 + 0x100) == '\0') {
      iVar13 = iVar23;
      if (iVar23 < 0xc4) {
        FUN_00684770(iVar23 * 0x274 + DAT_00fe1fa8,0x129);
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar16 - (uint)(*(ushort *)(iVar12 + 0x168) >> 1);
        *(int *)(iVar12 + 0x148) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x138) = uVar22;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar19 - (uint)*(ushort *)(iVar12 + 0x16a);
        *(int *)(iVar12 + 0x14c) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x13c) = uVar22;
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
        *(undefined4 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        uVar11 = FUN_0060a19c(iVar12 + 0x138,*(undefined2 *)(iVar12 + 0x168),
                              *(undefined2 *)(iVar12 + 0x16a));
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 100) = uVar11;
        FUN_00649e08(iVar23,1);
        iVar14 = local_164;
      }
      break;
    }
  }
  goto LAB_006b72bc;
  while( true ) {
    iVar23 = iVar23 + 1;
    iVar12 = iVar12 + 0x274;
    if (0xc3 < iVar23) break;
LAB_006b70a4:
    if (*(char *)(iVar12 + 0x100) == '\0') {
      iVar13 = iVar23;
      if (iVar23 < 0xc4) {
        FUN_00684770(iVar23 * 0x274 + DAT_00fe1fa8,0x12a);
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar16 - (uint)(*(ushort *)(iVar12 + 0x168) >> 1);
        *(int *)(iVar12 + 0x148) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x138) = uVar22;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar19 - (uint)*(ushort *)(iVar12 + 0x16a);
        *(int *)(iVar12 + 0x14c) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x13c) = uVar22;
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
        *(undefined4 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        uVar11 = FUN_0060a19c(iVar12 + 0x138,*(undefined2 *)(iVar12 + 0x168),
                              *(undefined2 *)(iVar12 + 0x16a));
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 100) = uVar11;
        FUN_00649e08(iVar23,1);
        iVar14 = local_164;
      }
      break;
    }
  }
  goto LAB_006b72bc;
  while( true ) {
    iVar23 = iVar23 + 1;
    iVar12 = iVar12 + 0x274;
    if (0xc3 < iVar23) break;
LAB_006b716a:
    if (*(char *)(iVar12 + 0x100) == '\0') {
      iVar13 = iVar23;
      if (iVar23 < 0xc4) {
        FUN_00684770(iVar23 * 0x274 + DAT_00fe1fa8,0x4a);
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar16 - (uint)(*(ushort *)(iVar12 + 0x168) >> 1);
        *(int *)(iVar12 + 0x148) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x138) = uVar22;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        iVar14 = iVar19 - (uint)*(ushort *)(iVar12 + 0x16a);
        *(int *)(iVar12 + 0x14c) = iVar14;
        uVar22 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar12 + 0x13c) = uVar22;
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
        *(undefined4 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
        iVar12 = iVar23 * 0x274 + DAT_00fe1fa8;
        uVar11 = FUN_0060a19c(iVar12 + 0x138,*(undefined2 *)(iVar12 + 0x168),
                              *(undefined2 *)(iVar12 + 0x16a));
        *(undefined1 *)(iVar23 * 0x274 + DAT_00fe1fa8 + 100) = uVar11;
        FUN_00649e08(iVar23,1);
        iVar14 = local_164;
      }
      break;
    }
  }
LAB_006b72bc:
  if (iVar13 != -1) goto LAB_006b74b4;
LAB_006b72c4:
  if ((local_138 != 0) && (iVar13 = FUN_006937c4(&local_120), iVar13 != -1)) goto LAB_006b74b4;
  if (local_16e[0] != '\0') {
    if (((local_148 == 0) && (DAT_010338d3 != '\0')) &&
       (iVar12 = FUN_00608330(&DAT_00fd67f4,7), iVar12 == 0)) {
      uVar22 = 0x3f5;
      iVar12 = FUN_00656328(0x57,0x3f5);
      if (iVar12 != 0) goto LAB_006b7320;
      iVar12 = FUN_00608330(&DAT_00fd67f4,3);
      if (iVar12 != 0) {
        uVar22 = 0x57;
      }
      uVar20 = 1;
    }
    else {
LAB_006b7320:
      uVar20 = 0;
      uVar22 = 0x30;
    }
    iVar13 = FUN_00685550(iVar16,iVar19,uVar22,uVar20);
    if (iVar13 != -1) goto LAB_006b74b4;
  }
  if (local_150 != 0) {
    if (DAT_010339a0 == 1) {
      iVar13 = FUN_00697070(iVar16,iVar19);
    }
    else if (DAT_010339a0 == 2) {
      iVar13 = FUN_00696e9c(iVar16,iVar19);
    }
    else {
      if (DAT_010339a0 != 3) goto LAB_006b7376;
      iVar13 = FUN_00696b8c(iVar16,iVar19);
    }
    if (iVar13 != -1) goto LAB_006b74b4;
  }
LAB_006b7376:
  if (local_16c <= DAT_00902ebc) {
    if (DAT_010703e0 == '\0') {
      if (DAT_010703e3 != '\0') {
        iVar13 = FUN_00696128(iVar16,iVar19);
LAB_006b73ae:
        if (iVar13 != -1) goto LAB_006b74b4;
      }
    }
    else if (DAT_010703e1 != '\0') {
      iVar13 = FUN_00695e5c(iVar16,iVar19);
      goto LAB_006b73ae;
    }
  }
  iVar13 = FUN_00694e70(&local_120);
  if (((((iVar13 == -1) && (iVar13 = FUN_00693bec(&local_120), iVar13 == -1)) &&
       ((iVar13 = FUN_006943e0(&local_120), iVar13 == -1 &&
        ((iVar13 = FUN_00694578(&local_120), iVar13 == -1 &&
         (iVar13 = FUN_00694ff4(&local_120), iVar13 == -1)))))) &&
      (iVar13 = FUN_006928f0(&local_120), iVar13 == -1)) &&
     ((iVar13 = FUN_00693198(&local_120), iVar13 == -1 &&
      (iVar13 = FUN_00695420(&local_120), iVar13 == -1)))) {
    if ((iVar14 == 0xe2) && (local_160 != 0)) {
      iVar12 = FUN_00608330(&DAT_00fd67f4,3);
      if (iVar12 == 0) {
        uVar22 = 0xe2;
      }
      else {
        uVar22 = 0xc6;
      }
      iVar13 = FUN_00685550(iVar16,iVar19,uVar22,0);
      if (iVar13 != -1) goto LAB_006b74b4;
    }
    iVar13 = FUN_006923e0(&local_120);
    if ((iVar13 == -1) && (iVar13 = FUN_006b2ea0(&local_120), iVar13 == -1)) {
      FUN_007ab078(&local_120,"No NPC was spawned!");
      FUN_007a7b00(&local_120,0x37,200,0x37,600);
      FUN_007aa754(&local_120);
      return;
    }
  }
LAB_006b74b4:
  if (iVar13 < 0xc4) {
    FUN_00649e80(iVar13,0);
  }
  return;
}

