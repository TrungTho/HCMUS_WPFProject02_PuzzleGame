   M S B A M L   `   `   `  ����R  NWPFProject02_GamePuzzle, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:WPFProject02_GamePuzzleWPFProject02_GamePuzzle  5   	   (    "WPFProject02_GamePuzzle.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   7local%clr-namespace:WPFProject02_GamePuzzle     5   	   -   5          � Title$  
MainWindow��5   	   $	��800��6   $	��700��6)    � WindowStartupLocation$ Manual=�5	   	    � Left$	 100��5
   	   
 � Top$ 20��6   .��5      �� -   5        $��canvasUI��5      
�������.+�5   
   ]� .+�5      o� $	��Top=�6   .@�5      m� $��_Option��6   .@�5      m� -   5        $��	_New game��5   5   m� 5      -   5        $��
_Save game��5   6   m� 5      -   5        $��
_Load game��5   6   �� 5      m� 5      -   5        $��_Exit��5      6.   5      m� 5      $��_Help��6   .@�5      m� -   5        $��_Auto Solve��5   3   m� 5      -   5        $��_Game Control��5   3   5       m� 5"      -	   5        $��_About��5"      6+   5%      5&      �� 5(   
   -
   5        $��textblockTimer��5(      $�� ��6*   
���  ��$
��Bold"�6C   $��30(�6U    �� TextAlignment$ Center=�6c   $	��100��6{   $��50��6�   
�����_� �� Left$ 37��6�   
 �� Top$
 1037��6�   �� 5*   
   -   5        $��labelControl��5*   $   $��	Controls:��6   $��30(�68   
���  ��$��Italic$�6W   $
��Bold"�6j   $ 37��6|   $
 1050��6�   �� 5,   
   -   5        $��buttonUp��5,      $��50��6!   $��50��6,   $ 37��6S   $
 1050��6d   .��5-      �� .+�5.      �� $��ImageButton/up.png��6   5/      50      �� 51   
   -   5        $��
buttonDown��51      $��50��6#   $��50��6.   $ 37��6U   $
 1050��6f   .��52      �� .+�53      �� $��ImageButton/down.png��6   54      55      �� 56   
   -   5        $��
buttonLeft��56      $��50��6#   $��50��6.   $ 37��6U   $
 1050��6f   .��57      �� .+�58      �� $��ImageButton/left.png��6   59      5:      �� 5<   
   -   5        $��buttonRight��5<      $��50��6$   $��50��6/   $ 37��6V   $
 1050��6g   .��5=      �� .+�5>      �� $��ImageButton/right.png��6   5?      5@      �� 5B   
   -   5        $��labelhighScore��5B   %   $��
HIGH SCORE��6   $��30(�6;   
���  ��$��Italic$�6Z   $
��Bold"�6m   $ 37��6   $
 1063��6�   �� 5D   
   -   5        $��listviewHighScore��5D      $	��180��6,   $	��200��68   $
 1000��6E   $��Disabled=�6W    �� View 5E      �� 5F      .P�5G      �� $	��No.��6%    �� Width$ 30��62   	 �� DisplayMemberBinding	 6=   �� *id+�� 5H      $
��User��6%   $	 100��63   	 6?   �� *
userName+�� 5I      $
��Time��6%   $ 50��63   	 6>   �� *
userTime+5J      5L      5M      5O      