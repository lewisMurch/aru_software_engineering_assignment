﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aru_software_eng_UI
{
	public partial class RelationshipManagerViewerUI : Form
	{
		FormManager manager;
		private int slot_counter = 0; //Keeps track of how many slots are use by buttons, globally - L
		private int button_base_size = 175; //Allows us to easily change the base size of a button - L
		private int button_spacing = 20; //Allows us to easily dicate how far apart buttons are form each other - L
		private int page_number = 1; //Keeps track of the page number
		int total_page_count; //Keeps track of the total amount of pages allowed - L
		List<InvestmentIdea> idea_list;

		public RelationshipManagerViewerUI(Form n_previous_window, List<InvestmentIdea> n_idea_list)
		{
			InitializeComponent();
			manager = new FormManager(n_previous_window, this);
			idea_list = n_idea_list;
			FancyDisplayBubbleTracker.getBubbleTracker().setLabel(DataOutputLabel2);
			firstTimeRun();
		}

		//Some temporary values for testing, mimicking the values being imported from the filter page - L
		int temp_min_cost = 2000;
		int temp_max_cost = 3000;
		int temp_min_risk = 10;
		int temp_max_risk = 400;

		
		int db_amount_of_entries = 17;




		//A function that sets some basic things up, only ran once at the start - L
		private void firstTimeRun()
		{
			pageLoader();
			//Finds the maximum number of pages - L
			total_page_count = ((db_amount_of_entries - 1) / 5) + 1;
			InvestmentIdeaDatabaseHandler.getInstance().loadInvestmentIdeasFromDatabaseToList();
		}

		//A function that clears the page - L
		private void pageClearer()
		{
			//Display the page number to the user - L
			page_number_label.Text = page_number.ToString();

			//Deletes all the buttons present on the screen. This allows for new ones to be loaded on the next page - L
			FancyDisplayBubbleTracker.deleteAllBubbles(this);

			//resets the slot counter to allow for properly displayed buttons on the next page - L
			slot_counter = 0;
		}

		//A function that loads the page - L
		private void pageLoader()
		{
			//For each page, find the appropraite starting value (e.g. if you are on page 3, start with index 11 as 5 bubbles per page) - L
			for (int i = (1 + ((page_number - 1) * 5)); i < (6 + ((page_number - 1) * 5)); i++)
			{
				if(spawnButton(i))
                {
					return;
                }
			}
		}

		//A function that finds the appropriate position for a given bubble - L
		private Point findPosition(int size)
		{
			//Create a new point called ret - L
			Point ret = new Point();

			//Store all the coordinates in an easy to read list, each line is a new coordinate - L
			int[] positions = { 320 - (size / 2), 276 - (size / 2), //Middle - L
			                    320 - (size / 2) - button_base_size - button_spacing, 276 - (size / 2) + button_base_size + button_spacing, //Bottom left - L
		                        320 - (size / 2) + button_base_size + button_spacing, 276 - (size / 2) + button_base_size + button_spacing, //Top right - L
								320 - (size / 2) - button_base_size - button_spacing, 276 - (size / 2) - button_base_size - button_spacing, //Top Left - L
								320 - (size / 2) + button_base_size + button_spacing, 276 - (size / 2) - button_base_size - button_spacing}; //Bottom Right - L

			List<int> position_list = new List<int>(positions);

			//Read the X and accompanying Y coordinate from the list
			ret.X = position_list[slot_counter];
			ret.Y = position_list[slot_counter + 1];

			//Incriment the slot counter twice so we know how many bubbles are on the page - L
			//We do this twice so we can get the X and Y pos from the list - L
			slot_counter++;
			slot_counter++;

			//return the position of the bubble - L
			return ret;
		}

		//A function that takes the filter results from the previous page and compares them
		//to the inputted results from the database to result in a level of suitability from 0 - 100 - L
		private int buttonSizeCalcualtor(int min_cost_filter, int max_cost_filter, int min_risk_filter, 
			int max_risk_filter, int db_cost, int db_risk)
		{
			//Find the middle point of the toggles to use in the suitability calculations - L
			float av_cost_filter = min_cost_filter * max_cost_filter / 2;
			float av_risk_filter = min_risk_filter * max_risk_filter / 2;

			//Find the % difference. The closer to 0, the closer to a full size button - L
			float suitability_cost = db_cost / av_cost_filter;
			suitability_cost = System.Math.Abs(suitability_cost - 1);
			float suitability_risk = db_risk / av_risk_filter;
			suitability_risk = System.Math.Abs(suitability_risk - 1);

			//Find the avarage value between the two % differences - L
			float av_suitability = suitability_cost * suitability_risk / 2;

			//Make it so the closer the number is to 0, the closer it now it to 1
			av_suitability = 1 - av_suitability;

			//The closer to 1 the number is, the closer to full size it will be - L
			float final_suitability = av_suitability * button_base_size;

			return (int)final_suitability;
		}

		/*A function that spawns in a button - L
		Returns true if more buttons should keep being made.
		 */
		private bool spawnButton(int button_index_from_list)
		{
			if(InvestmentIdeaDatabaseHandler.getInstance().getHighestID("ID") < button_index_from_list)
            {
				Console.WriteLine("error, index out of bounds, " + button_index_from_list + "evading.");
				return false;
            }
			//Calculates a buttons size based of values inputted from the database, and the filters previously selected - L

			int getCost = (int)idea_list[button_index_from_list].getCost();
			int getRisk = idea_list[button_index_from_list].getRiskLevel();

			int size_of_button = buttonSizeCalcualtor(temp_min_cost, temp_max_cost, temp_min_risk, temp_max_risk, getCost, getRisk);
			
			//Finds a buttons location based off of it's size - L
			Point location_of_button = findPosition(size_of_button);

			//safeguard againes OOB exceptions.
			if(idea_list.Count <= button_index_from_list)
            {
				Console.WriteLine("idea_list.Count <= button_index_from_list");
				return true;
            }
			FancyDisplayBubbleTracker.instanceAddBubble(new Button(), idea_list[button_index_from_list]);
			this.Controls.Add(FancyDisplayBubbleTracker.instanceGetLastBubble().getButton()); //Add controls to the recently created button - L
			FancyDisplayBubbleTracker.instanceGetLastBubble().getButton().Text = idea_list[button_index_from_list].getName(); //Sets the text of the button - L
			FancyDisplayBubbleTracker.instanceGetLastBubble().getButton().Location = location_of_button; //Sets the location of the button - L
			FancyDisplayBubbleTracker.instanceGetLastBubble().getButton().Size = new Size(size_of_button, size_of_button); //Sets the size of button to the default size - L
			FancyDisplayBubbleTracker.instanceGetLastBubble().getButton().Font = new Font("Agency FB", size_of_button / 6, FontStyle.Bold);
			
			return false;
		}

		//If the left button is pressed, clear all bubbles and update the page and add the new bubbles - L
		private void page_left_button_Click(object sender, EventArgs e)
		{
			if (page_number > 1)
			{
				page_number--;
				pageClearer();
				pageLoader();
			}

		}

		//If the left button is pressed, clear all bubbles and update the page and add the new bubbles - L
		private void page_right_button_Click(object sender, EventArgs e)
		{
			if (page_number < total_page_count)
			{
				page_number++;
				pageClearer();
				pageLoader();
			}

		}

        private void IS_Back_Button_Click(object sender, EventArgs e)
        {
			manager.back();
        }

        private void DataOutputLabel_Click(object sender, EventArgs e)
        {

        }

        private void RelationshipManagerViewerUI_Load(object sender, EventArgs e)
        {

        }
    }
}
