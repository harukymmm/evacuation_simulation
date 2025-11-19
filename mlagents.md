# 実行方法

Python環境：3.10.12
condaで環境構築を行なっている

'''
eval "$(/opt/homebrew/anaconda3/bin/conda shell.zsh hook)"
conda activate mlagents

source ~/miniconda3/bin/activate mlagents
mlagents-learn Assets/Config/Tutorial-1.yaml --run-id=shelter_training --force
'''
