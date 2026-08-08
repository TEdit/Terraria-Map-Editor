
int * FUN_007783c8(int *param_1,undefined4 param_2)

{
  void *pvVar1;
  int *piVar2;
  undefined4 uVar3;
  int *piVar4;
  undefined4 uVar5;
  
  pvVar1 = malloc(0x9c);
  if (pvVar1 == (void *)0x0) {
    piVar2 = (int *)0x0;
  }
  else {
    *(undefined4 *)((int)pvVar1 + 0x3c) = 0;
    *(undefined4 *)((int)pvVar1 + 0x88) = 0;
    piVar2 = (int *)FUN_005f544c(pvVar1,param_2,0);
  }
  uVar3 = (**(code **)(*piVar2 + 4))(piVar2);
  (**(code **)(*param_1 + 0x24))(param_1,uVar3,param_2,*(code **)(*param_1 + 0x24),pvVar1);
  pvVar1 = malloc(0x9c);
  if (pvVar1 == (void *)0x0) {
    piVar4 = (int *)0x0;
  }
  else {
    *(undefined4 *)((int)pvVar1 + 0x3c) = 0;
    *(undefined4 *)((int)pvVar1 + 0x88) = 0;
    piVar4 = (int *)FUN_005f544c(pvVar1,param_2,0);
  }
  (**(code **)(*piVar4 + 8))(piVar4,param_2);
  uVar3 = (**(code **)(*piVar4 + 4))(piVar4);
  uVar5 = (**(code **)(*piVar2 + 4))(piVar2);
  FUN_004f8d18(&DAT_00a59380,uVar5,uVar3,param_2,pvVar1);
  (**(code **)*piVar2)(piVar2,1);
  return piVar4;
}

