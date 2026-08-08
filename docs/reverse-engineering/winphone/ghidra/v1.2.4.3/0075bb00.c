
void FUN_0075bb00(void)

{
  float fVar1;
  byte bVar2;
  int *piVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  ushort uVar7;
  byte *pbVar8;
  byte *pbVar9;
  byte *pbVar10;
  short sVar11;
  byte *pbVar12;
  uint uVar13;
  uint uVar14;
  uint in_fpscr;
  float fVar15;
  float fVar16;
  float fVar17;
  longlong lVar18;
  uint local_44;
  byte local_40;
  byte local_3f;
  byte local_3e;
  byte local_3d;
  byte local_3c;
  byte local_3b;
  byte local_3a;
  byte local_39;
  byte local_38 [2];
  ushort local_36 [9];
  
  lVar18 = FUN_0082a0b8();
  fVar1 = DAT_0075bc38;
  iVar6 = (int)((ulonglong)lVar18 >> 0x20);
  piVar3 = (int *)lVar18;
  local_44 = 0;
  if (0 < DAT_00934f08) {
    do {
      if ((local_44 & 0x1f) == 0) {
        iVar4 = FUN_00564548();
        fVar16 = (float)VectorSignedToFloat(local_44,(byte)(in_fpscr >> 0x16) & 3);
        fVar15 = (float)VectorSignedToFloat((int)DAT_00934f08,(byte)(in_fpscr >> 0x16) & 3);
        fVar15 = (fVar16 / fVar15) * fVar1;
        uVar13 = in_fpscr & 0xfffffff | (uint)(fVar15 < 1.0) << 0x1f | (uint)(fVar15 == 1.0) << 0x1e
        ;
        in_fpscr = uVar13 | (uint)NAN(fVar15) << 0x1c;
        *(float *)(iVar4 + 0x2e98) = fVar15;
        bVar2 = (byte)(uVar13 >> 0x18);
        if (!(bool)(bVar2 >> 6 & 1) && bVar2 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar4 + 0x2e98) = 1.0;
        }
      }
      iVar4 = 0;
      pbVar12 = (byte *)(DAT_009751c0 + DAT_009751cc * local_44 * 0x10);
      if (0 < DAT_00934f0c) {
        do {
          *pbVar12 = 0;
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_3e,1);
          bVar2 = (pbVar12[1] ^ local_3e) & 1 ^ pbVar12[1];
          pbVar12[1] = bVar2;
          bVar2 = bVar2 ^ (bVar2 ^ local_3e) & 2;
          pbVar12[1] = bVar2;
          bVar2 = bVar2 ^ (bVar2 ^ local_3e) & 4;
          pbVar12[1] = bVar2;
          *(ushort *)(pbVar12 + 4) = *(ushort *)(pbVar12 + 4) & 0xfff8 | local_3e >> 3 & 7;
          bVar2 = (bVar2 ^ local_3e) & 0x7f ^ local_3e;
          pbVar12[1] = bVar2;
          if ((bVar2 & 1) != 0) {
            if (lVar18 < 0x3b00000000) {
              (**(code **)(*piVar3 + 0x24))(piVar3,&local_39,1);
              uVar7 = *(ushort *)(pbVar12 + 8) ^ (ushort)local_39;
            }
            else {
              (**(code **)(*piVar3 + 0x24))(piVar3,local_36,2);
              uVar7 = *(ushort *)(pbVar12 + 8) ^ local_36[0];
            }
            uVar7 = uVar7 & 0x1ff ^ *(ushort *)(pbVar12 + 8);
            uVar13 = uVar7 & 0x1ff;
            *(ushort *)(pbVar12 + 8) = uVar7;
            if (uVar13 == 0x7f) {
              pbVar12[1] = pbVar12[1] & 0xfe;
            }
            if (((&DAT_00a0fb80)[uVar13 * 4] & 0x10000) == 0) {
              sVar11 = -1;
              pbVar12[0xc] = 0xff;
              pbVar12[0xd] = 0xff;
LAB_0075bce2:
              *(short *)(pbVar12 + 0xe) = sVar11;
            }
            else {
              (**(code **)(*piVar3 + 0x24))(piVar3,pbVar12 + 0xc,2);
              (**(code **)(*piVar3 + 0x24))(piVar3,pbVar12 + 0xe,2);
              FUN_00751558(pbVar12,iVar6);
              uVar7 = *(ushort *)(pbVar12 + 8);
              if ((uVar7 & 0x1ff) == 0x13) {
                if (-1 < *(short *)(pbVar12 + 0xe)) goto LAB_0075bce4;
                sVar11 = 0;
                goto LAB_0075bce2;
              }
              if ((uVar7 & 0x1ff) == 0x90) {
                sVar11 = 0;
                goto LAB_0075bce2;
              }
              if (lVar18 < 0x3a00000000) {
                if (uVar13 == 0x23) {
                  sVar11 = *(short *)(pbVar12 + 0xe) + 0x36;
                }
                else {
                  if (uVar13 != 0x24) goto LAB_0075bce4;
                  sVar11 = *(short *)(pbVar12 + 0xe) + 0x6c;
                }
                goto LAB_0075bce2;
              }
            }
LAB_0075bce4:
            *(ushort *)(pbVar12 + 8) = uVar7 & 0xfe00 | (ushort)uVar13;
            (**(code **)(*piVar3 + 0x24))(piVar3,&local_3c,1);
            pbVar12[2] = local_3c;
          }
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_3d,1);
          if (local_3d != 0) {
            (**(code **)(*piVar3 + 0x24))(piVar3,&local_3a,1);
            pbVar12[3] = local_3a;
          }
          pbVar12[10] = local_3d;
          (**(code **)(*piVar3 + 0x24))(piVar3,pbVar12 + 6,1);
          if (pbVar12[6] != 0) {
            (**(code **)(*piVar3 + 0x24))(piVar3,local_38,1);
            *(ushort *)(pbVar12 + 4) =
                 *(ushort *)(pbVar12 + 4) ^
                 (*(ushort *)(pbVar12 + 4) ^ (ushort)local_38[0] << 3) & 0x18;
          }
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_40,1);
          *pbVar12 = local_40 & 0x10 | *pbVar12;
          bVar2 = (pbVar12[1] ^ local_40 >> 2) & 8 ^ pbVar12[1];
          pbVar12[1] = bVar2;
          pbVar12[1] = bVar2 ^ (bVar2 ^ local_40 >> 2) & 0x10;
          if (iVar6 - 0x3cU < 9) {
            (**(code **)(*piVar3 + 0x24))(piVar3,&local_3b,1);
            *(ushort *)(pbVar12 + 8) = *(ushort *)(pbVar12 + 8) & 0xe1ff | (local_3b & 0xfff0) << 5;
          }
          iVar5 = FUN_007cf070();
          if (iVar5 != 0) {
            *(ushort *)(pbVar12 + 8) = *(ushort *)(pbVar12 + 8) & 0xe1ff;
          }
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_40,1);
          uVar13 = (uint)local_40;
          if ((local_40 & 0x80) != 0) {
            (**(code **)(*piVar3 + 0x24))(piVar3,&local_40,1);
            uVar13 = uVar13 & 0x7f | (uint)local_40 << 7;
          }
          if (uVar13 != 0) {
            if (uVar13 != 0) {
              pbVar8 = pbVar12;
              pbVar9 = pbVar12 + 0x10;
              do {
                pbVar10 = pbVar9 + 4;
                *(undefined4 *)pbVar9 = *(undefined4 *)pbVar8;
                pbVar8 = pbVar8 + 4;
                pbVar9 = pbVar10;
              } while (pbVar10 != pbVar12 + 0x10 + uVar13 * 0x10);
            }
            pbVar12 = pbVar12 + uVar13 * 0x10;
          }
          iVar4 = iVar4 + uVar13 + 1;
          pbVar12 = pbVar12 + 0x10;
        } while (iVar4 < DAT_00934f0c);
      }
      local_44 = local_44 + 1;
    } while ((int)local_44 < (int)DAT_00934f08);
  }
  fVar15 = DAT_0075bf94;
  if ((0x44ffffffff < lVar18) && (uVar13 = 0, 0 < DAT_00934f08)) {
    do {
      if ((uVar13 & 0x1f) == 0) {
        iVar6 = FUN_00564548();
        fVar17 = (float)VectorSignedToFloat(uVar13,(byte)(in_fpscr >> 0x16) & 3);
        fVar16 = (float)VectorSignedToFloat((int)DAT_00934f08,(byte)(in_fpscr >> 0x16) & 3);
        fVar16 = fVar1 + (fVar17 / fVar16) * fVar15;
        uVar14 = in_fpscr & 0xfffffff | (uint)(fVar16 < 1.0) << 0x1f | (uint)(fVar16 == 1.0) << 0x1e
        ;
        in_fpscr = uVar14 | (uint)NAN(fVar16) << 0x1c;
        *(float *)(iVar6 + 0x2e98) = fVar16;
        bVar2 = (byte)(uVar14 >> 0x18);
        if (!(bool)(bVar2 >> 6 & 1) && bVar2 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar6 + 0x2e98) = 1.0;
        }
      }
      iVar4 = 0;
      iVar6 = DAT_009751c0 + DAT_009751cc * uVar13 * 0x10;
      if (0 < DAT_00934f0c) {
        do {
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_3f,1);
          *(ushort *)(iVar6 + 8) = *(ushort *)(iVar6 + 8) & 0xe1ff | (local_3f & 0xfff0) << 5;
          (**(code **)(*piVar3 + 0x24))(piVar3,&local_3f,1);
          uVar14 = (uint)local_3f;
          if ((local_3f & 0x80) != 0) {
            (**(code **)(*piVar3 + 0x24))(piVar3,&local_3f,1);
            uVar14 = uVar14 & 0x7f | (uint)local_3f << 7;
          }
          iVar4 = iVar4 + uVar14;
          if (uVar14 != 0) {
            do {
              uVar14 = uVar14 - 1;
              *(ushort *)(iVar6 + 0x18) =
                   *(ushort *)(iVar6 + 0x18) ^
                   (*(ushort *)(iVar6 + 8) ^ *(ushort *)(iVar6 + 0x18)) & 0x1e00;
              iVar6 = iVar6 + 0x10;
            } while (0 < (int)uVar14);
          }
          iVar4 = iVar4 + 1;
          iVar6 = iVar6 + 0x10;
        } while (iVar4 < DAT_00934f0c);
      }
      uVar13 = uVar13 + 1;
    } while ((int)uVar13 < (int)DAT_00934f08);
  }
  FUN_0082a0d0();
  return;
}

