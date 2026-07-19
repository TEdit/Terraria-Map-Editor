
void FUN_0075c298(void)

{
  float fVar1;
  ushort uVar2;
  char cVar3;
  int *piVar4;
  int iVar5;
  uint *puVar6;
  int iVar7;
  uint uVar8;
  uint uVar9;
  int iVar10;
  byte bVar11;
  int iVar12;
  int iVar13;
  int iVar14;
  uint uVar15;
  uint in_fpscr;
  float fVar16;
  float fVar17;
  float fVar18;
  undefined4 local_4c;
  undefined4 local_48;
  undefined4 local_44;
  undefined4 local_40;
  undefined2 local_3c;
  
  piVar4 = (int *)FUN_0082a0b8();
  fVar1 = DAT_0075c364;
  uVar15 = 0;
  if (0 < DAT_00934f08) {
    do {
      if ((uVar15 & 0x1f) == 0) {
        iVar5 = FUN_00564548();
        fVar17 = (float)VectorSignedToFloat(uVar15,(byte)(in_fpscr >> 0x16) & 3);
        fVar16 = (float)VectorSignedToFloat((int)DAT_00934f08,(byte)(in_fpscr >> 0x16) & 3);
        fVar16 = (fVar17 / fVar16) * fVar1;
        uVar8 = in_fpscr & 0xfffffff | (uint)(fVar16 < 1.0) << 0x1f | (uint)(fVar16 == 1.0) << 0x1e;
        in_fpscr = uVar8 | (uint)NAN(fVar16) << 0x1c;
        *(float *)(iVar5 + 0x2e98) = fVar16;
        bVar11 = (byte)(uVar8 >> 0x18);
        if (!(bool)(bVar11 >> 6 & 1) && bVar11 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar5 + 0x2e98) = 1.0;
        }
      }
      iVar5 = 0;
      if (0 < DAT_00934f0c) {
        do {
          puVar6 = (uint *)(DAT_009751c0 + (DAT_009751cc * uVar15 + iVar5) * 0x10);
          local_44 = puVar6[2];
          uVar9 = *puVar6;
          local_48 = puVar6[1];
          local_40 = puVar6[3];
          uVar8 = local_44 & 0x1ff;
          local_4c._1_1_ = (byte)(uVar9 >> 8);
          bVar11 = local_4c._1_1_;
          if (uVar8 == 0x7f) {
            bVar11 = local_4c._1_1_ & 0xfe;
            local_4c = uVar9 & 0xfffffeff;
            uVar9 = local_4c;
          }
          local_4c = uVar9;
          uVar2 = (ushort)local_48;
          local_3c = CONCAT11(local_3c._1_1_,bVar11 & 0x87 | (byte)((local_48 & 7) << 3));
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          if ((bVar11 & 1) != 0) {
            local_3c = (ushort)uVar8;
            (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,2);
            if (((&DAT_00a0fb80)[uVar8 * 4] & 0x10000) != 0) {
              local_3c = (undefined2)local_40;
              (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,2);
              local_3c = local_40._2_2_;
              (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,2);
            }
            local_3c = CONCAT11(local_3c._1_1_,local_4c._2_1_);
            (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          }
          cVar3 = local_44._2_1_;
          local_3c._0_1_ = local_44._2_1_;
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          if (cVar3 != '\0') {
            local_3c._0_1_ = local_4c._3_1_;
            (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          }
          cVar3 = local_48._2_1_;
          local_3c._0_1_ = local_48._2_1_;
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          if (cVar3 != '\0') {
            local_3c._0_1_ = (byte)(uVar2 >> 3) & 3;
            (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          }
          local_3c = CONCAT11(local_3c._1_1_,(byte)local_4c & 0x10 | (bVar11 & 0x18) << 2);
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          iVar10 = 1;
          iVar13 = iVar5 + 1;
          iVar14 = (int)DAT_00934f0c;
          if (iVar13 < iVar14) {
            iVar12 = DAT_009751c0 + (DAT_009751cc * uVar15 + iVar5 + 1) * 0x10;
            do {
              iVar7 = FUN_00751fd0(&local_4c,iVar12);
              if (iVar7 == 0) break;
              iVar13 = iVar13 + 1;
              iVar10 = iVar10 + 1;
              iVar12 = iVar12 + 0x10;
            } while (iVar13 < iVar14);
          }
          iVar10 = iVar10 + -1;
          if (iVar10 < 0x80) {
            local_3c = CONCAT11(local_3c._1_1_,(char)iVar10);
          }
          else {
            local_3c = CONCAT11(local_3c._1_1_,(char)iVar10) | 0x80;
            (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
            local_3c = CONCAT11(local_3c._1_1_,(char)(iVar10 >> 7));
          }
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          iVar5 = iVar5 + iVar10 + 1;
        } while (iVar5 < DAT_00934f0c);
      }
      uVar15 = uVar15 + 1;
    } while ((int)uVar15 < (int)DAT_00934f08);
  }
  fVar16 = DAT_0075c67c;
  uVar15 = 0;
  if (0 < DAT_00934f08) {
    do {
      if ((uVar15 & 0x1f) == 0) {
        iVar5 = FUN_00564548();
        fVar18 = (float)VectorSignedToFloat(uVar15,(byte)(in_fpscr >> 0x16) & 3);
        fVar17 = (float)VectorSignedToFloat((int)DAT_00934f08,(byte)(in_fpscr >> 0x16) & 3);
        fVar17 = fVar1 + (fVar18 / fVar17) * fVar16;
        uVar8 = in_fpscr & 0xfffffff | (uint)(fVar17 < 1.0) << 0x1f | (uint)(fVar17 == 1.0) << 0x1e;
        in_fpscr = uVar8 | (uint)NAN(fVar17) << 0x1c;
        *(float *)(iVar5 + 0x2e98) = fVar17;
        bVar11 = (byte)(uVar8 >> 0x18);
        if (!(bool)(bVar11 >> 6 & 1) && bVar11 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar5 + 0x2e98) = 1.0;
        }
        iVar5 = FUN_00564548();
        FUN_004528ac("Tile::Save: %f \n",*(undefined4 *)(iVar5 + 0x2e98));
      }
      iVar5 = 0;
      if (0 < DAT_00934f0c) {
        do {
          puVar6 = (uint *)(DAT_009751c0 + (DAT_009751cc * uVar15 + iVar5) * 0x10);
          uVar8 = puVar6[2];
          local_4c = *puVar6;
          local_48 = puVar6[1];
          local_40 = puVar6[3];
          local_44 = uVar8;
          local_3c._0_1_ = (char)((uVar8 >> 9 & 0xf) << 4);
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          iVar10 = iVar5 + 1;
          iVar13 = 1;
          if (iVar10 < DAT_00934f0c) {
            iVar14 = DAT_009751c0 + (DAT_009751cc * uVar15 + iVar5 + 1) * 0x10;
            do {
              if ((uVar8 & 0x1fff) >> 9 != (*(ushort *)(iVar14 + 8) & 0x1fff) >> 9) break;
              iVar10 = iVar10 + 1;
              iVar13 = iVar13 + 1;
              iVar14 = iVar14 + 0x10;
            } while (iVar10 < DAT_00934f0c);
          }
          iVar13 = iVar13 + -1;
          if (iVar13 < 0x80) {
            local_3c = CONCAT11(local_3c._1_1_,(char)iVar13);
          }
          else {
            local_3c = CONCAT11(local_3c._1_1_,(char)iVar13) | 0x80;
            (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
            local_3c = CONCAT11(local_3c._1_1_,(char)(iVar13 >> 7));
          }
          (**(code **)(*piVar4 + 0x20))(piVar4,&local_3c,1);
          iVar5 = iVar5 + iVar13 + 1;
        } while (iVar5 < DAT_00934f0c);
      }
      uVar15 = uVar15 + 1;
    } while ((int)uVar15 < (int)DAT_00934f08);
  }
  FUN_0082a0d0();
  return;
}

